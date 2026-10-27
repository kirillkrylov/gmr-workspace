using System;
using Common.Logging;
using Terrasoft.Core;
using CoreSysSettings = Terrasoft.Core.Configuration.SysSettings;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// Entry point of the revenue feed for the business process and the platform listeners.
	/// </summary>
	public static class RevenueFeedService {

		/// <summary>Code of the business process that processes one revenue feed run.</summary>
		public const string ProcessName = "RevenueFeedRun_ProcessFile";

		/// <summary>Process parameter that carries the run id.</summary>
		public const string RunIdParameter = "RunIdParameter";

		internal const string IntervalSettingCode = "RevenueFeedIntervalMinutes";

		/// <summary>
		/// Runs the stage that a process element of the UsrRunRevenueFeedStage user task stands for: the element's name
		/// selects the stage. Throws only when the process is wired wrongly, so that the process log shows the cause.
		/// </summary>
		public static RevenueFeedStageOutcome ExecuteStage(UserConnection userConnection, string elementName, Guid runId) {
			Guid? stageId = RevenueFeedIds.StageForElement(elementName);
			if (!stageId.HasValue) {
				throw new InvalidOperationException(
					$"Process element \"{elementName}\" is not a revenue feed stage. Rename it to one of the stage names.");
			}
			if (runId == Guid.Empty) {
				throw new InvalidOperationException(
					$"The RunId parameter of element \"{elementName}\" is empty; map {RunIdParameter} to it.");
			}
			return CreatePipeline(userConnection).RunStage(runId, stageId.Value);
		}

		/// <summary>
		/// The start of the delivery slot that a time in the user's time zone belongs to, in the user's time zone.
		/// Slots are aligned to midnight of the feed time zone, so every user and the missed-slot job agree on them.
		/// </summary>
		internal static DateTime GetSlot(DateTime userTime, TimeZoneInfo userTimeZone, TimeZoneInfo feedTimeZone,
				int intervalMinutes) {
			DateTime utc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(userTime, DateTimeKind.Unspecified), userTimeZone);
			DateTime feedTime = TimeZoneInfo.ConvertTimeFromUtc(utc, feedTimeZone);
			DateTime feedSlot = RevenueFeedPipeline.GetSlotStart(feedTime, intervalMinutes);
			DateTime slotUtc = TimeZoneInfo.ConvertTimeToUtc(feedSlot, feedTimeZone);
			return TimeZoneInfo.ConvertTimeFromUtc(slotUtc, userTimeZone);
		}

		/// <summary>The feed time zone: the time zone of the system user, which also runs the missed-slot job.</summary>
		internal static TimeZoneInfo GetFeedTimeZone(UserConnection userConnection) {
			return userConnection.AppConnection?.SystemUserConnection?.CurrentUser?.TimeZone
				?? userConnection.CurrentUser.TimeZone;
		}

		internal static IRevenueFeedPipeline CreatePipeline(UserConnection userConnection) {
			int interval = CoreSysSettings.GetValue(userConnection, IntervalSettingCode, 120);
			TimeZoneInfo userTimeZone = userConnection.CurrentUser.TimeZone;
			TimeZoneInfo feedTimeZone = GetFeedTimeZone(userConnection);
			var options = new RevenueFeedOptions {
				IntervalMinutes = interval,
				GetSlot = time => GetSlot(time, userTimeZone, feedTimeZone, interval)
			};
			return new RevenueFeedPipeline(new RevenueFeedStore(userConnection), new RevenueFeedNotifier(userConnection),
				options, new RevenueFeedLock(), () => userConnection.CurrentUser.GetCurrentDateTime(),
				BatchFileProcessingApp.Instance.GetRequiredService<ILog>());
		}
	}
}
