using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Scheduler;
using CoreSysSettings = Terrasoft.Core.Configuration.SysSettings;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// Records a Missed run for every delivery slot that ended without a file, so gaps show on the runs board and
	/// administrators are notified. A slot counts as missed once its grace period after the slot end has passed.
	/// After downtime the job catches up on every overdue slot back to <see cref="MaxBackfillHours"/>, but never before
	/// the first slot the feed has a run for, so a fresh installation does not report a day of misses.
	/// Runs as the system user, whose time zone is the feed time zone.
	/// </summary>
	public sealed class RevenueFeedMissedRunJob : IJobExecutor {

		internal const string JobGroup = "RevenueFeed";
		internal const string GraceSettingCode = "RevenueFeedGraceMinutes";
		internal const int MaxBackfillHours = 24;

		public void Execute(UserConnection userConnection, IDictionary<string, object> parameters) {
			int interval = CoreSysSettings.GetValue(userConnection, RevenueFeedService.IntervalSettingCode, 120);
			int grace = CoreSysSettings.GetValue(userConnection, GraceSettingCode, 15);
			DateTime now = userConnection.CurrentUser.GetCurrentDateTime();
			DateTime latest = GetSlotToCheck(now, interval, grace);
			List<DateTime> knownSlots = GetRunSlotsSince(userConnection, latest.AddHours(-MaxBackfillHours));
			DateTime? firstRunSlot = GetFirstRunSlot(userConnection);
			foreach (DateTime slot in GetMissedSlots(latest, interval, knownSlots, firstRunSlot)) {
				RecordMissedRun(userConnection, slot, interval, grace, now);
			}
		}

		/// <summary>
		/// The latest slot whose grace period has ended.
		/// </summary>
		internal static DateTime GetSlotToCheck(DateTime now, int intervalMinutes, int graceMinutes) {
			DateTime currentSlot = RevenueFeedPipeline.GetSlotStart(now.AddMinutes(-Math.Max(0, graceMinutes)), intervalMinutes);
			return currentSlot.AddMinutes(-Math.Max(1, intervalMinutes));
		}

		/// <summary>
		/// Overdue slots without a run, oldest first: from the later of <see cref="MaxBackfillHours"/> ago and the first
		/// slot the feed has a run for, up to <paramref name="latest"/>. With no runs at all only the latest slot counts.
		/// </summary>
		internal static IEnumerable<DateTime> GetMissedSlots(DateTime latest, int intervalMinutes,
				ICollection<DateTime> knownSlots, DateTime? firstRunSlot) {
			int interval = Math.Max(1, intervalMinutes);
			DateTime from = firstRunSlot.HasValue ? firstRunSlot.Value : latest;
			DateTime limit = latest.AddHours(-MaxBackfillHours);
			if (from < limit) {
				from = RevenueFeedPipeline.GetSlotStart(limit, interval);
			}
			var known = new HashSet<DateTime>(knownSlots);
			for (DateTime slot = from; slot <= latest; slot = slot.AddMinutes(interval)) {
				if (!known.Contains(slot)) {
					yield return slot;
				}
			}
		}

		private static void RecordMissedRun(UserConnection userConnection, DateTime slot, int interval, int grace,
				DateTime now) {
			Guid runId = Guid.NewGuid();
			string message = $"No file was received for the {slot:HH:mm}–{slot.AddMinutes(interval):HH:mm} slot " +
				$"of {slot:yyyy-MM-dd} (grace period {grace} min).";
			Entity run = userConnection.EntitySchemaManager.GetInstanceByName(RevenueFeedStore.RunSchemaName)
				.CreateEntity(userConnection);
			run.SetDefColumnValues();
			run.SetColumnValue("Id", runId);
			run.SetColumnValue("ScheduledOn", slot);
			run.SetColumnValue("CompletedOn", now);
			run.SetColumnValue("StatusId", RevenueFeedIds.RunStatus.Missed);
			run.SetColumnValue("ErrorCodeId", RevenueFeedIds.ErrorCode.FeedMissed);
			run.SetColumnValue("ErrorMessage", message);
			if (!run.Save(false)) {
				throw new InvalidOperationException($"Saving the missed run for slot {slot:yyyy-MM-dd HH:mm} was cancelled.");
			}
			new RevenueFeedNotifier(userConnection).NotifyProblem(runId, run.GetTypedColumnValue<string>("Name"),
				RevenueFeedIds.RunStatus.Missed, message);
		}

		private static List<DateTime> GetRunSlotsSince(UserConnection userConnection, DateTime since) {
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, RevenueFeedStore.RunSchemaName);
			EntitySchemaQueryColumn scheduledOn = esq.AddColumn("ScheduledOn");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.GreaterOrEqual, "ScheduledOn", since));
			return esq.GetEntityCollection(userConnection)
				.Select(e => e.GetTypedColumnValue<DateTime>(scheduledOn.Name)).ToList();
		}

		private static DateTime? GetFirstRunSlot(UserConnection userConnection) {
			var esq = new EntitySchemaQuery(userConnection.EntitySchemaManager, RevenueFeedStore.RunSchemaName);
			esq.RowCount = 1;
			EntitySchemaQueryColumn scheduledOn = esq.AddColumn("ScheduledOn");
			scheduledOn.OrderByAsc();
			esq.Filters.Add(esq.CreateIsNotNullFilter("ScheduledOn"));
			Entity first = esq.GetEntityCollection(userConnection).FirstOrDefault();
			return first == null ? (DateTime?)null : first.GetTypedColumnValue<DateTime>(scheduledOn.Name);
		}
	}
}
