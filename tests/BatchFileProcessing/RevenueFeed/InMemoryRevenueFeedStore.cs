using System;
using System.Collections.Generic;
using System.Linq;
using BatchFileProcessingApp.RevenueFeed;

namespace BatchFileProcessing.Tests.RevenueFeed {

	/// <summary>
	/// In-memory <see cref="IRevenueFeedStore"/> with the same semantics as the database store.
	/// </summary>
	internal sealed class InMemoryRevenueFeedStore : IRevenueFeedStore {

		public sealed class Run {
			public Guid Id;
			public string Name;
			public Guid StatusId = RevenueFeedIds.RunStatus.WaitingForFile;
			public Guid? StageId;
			public DateTime? ScheduledOn;
			public DateTime? ReceivedOn;
			public DateTime? StartedOn;
			public DateTime? CompletedOn;
			public int Duration;
			public string FileName;
			public string FileHash;
			public int LineCount;
			public int AppliedCount;
			public int RejectedCount;
			public int SkippedCount;
			public int Attempt;
			public Guid? ErrorCodeId;
			public string ErrorMessage;
			public Guid? FileId;
			public RevenueFeedFile File;
		}

		public sealed class Opportunity {
			public Guid Id;
			public string Code;
			public Guid AccountId;
			public decimal Amount;
			public decimal ActualAmount;
			public decimal Variance;
			public decimal VariancePercent;
			public Guid? LastRunId;
		}

		public readonly Dictionary<Guid, Run> Runs = new Dictionary<Guid, Run>();
		public readonly List<RevenueFeedStepRecord> Steps = new List<RevenueFeedStepRecord>();
		public readonly Dictionary<Guid, RevenueFeedLineRecord> Lines = new Dictionary<Guid, RevenueFeedLineRecord>();
		public readonly Dictionary<string, Guid> Accounts = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
		public readonly Dictionary<Guid, decimal> AccountActuals = new Dictionary<Guid, decimal>();
		public readonly List<Opportunity> Opportunities = new List<Opportunity>();
		public bool FailNextLineSave;
		public int FailLineSaveAfter = -1;
		public bool FailOpportunityActual;
		public readonly Dictionary<Guid, RevenueFeedFile> Files = new Dictionary<Guid, RevenueFeedFile>();

		public Run AddRun(string name, RevenueFeedFile file = null) {
			var run = new Run { Id = Guid.NewGuid(), Name = name };
			Runs.Add(run.Id, run);
			Attach(run, file);
			return run;
		}

		/// <summary>Uploads a file to the run; it becomes the run's latest file.</summary>
		public void Attach(Run run, RevenueFeedFile file) {
			if (file == null) {
				return;
			}
			var copy = new RevenueFeedFile { Id = Guid.NewGuid(), Name = file.Name, Content = file.Content };
			Files.Add(copy.Id, copy);
			run.File = copy;
		}

		public RevenueFeedRunInfo FindRun(Guid runId) {
			if (!Runs.TryGetValue(runId, out Run run)) {
				return null;
			}
			return new RevenueFeedRunInfo {
				Id = run.Id, Name = run.Name, StatusId = run.StatusId, ScheduledOn = run.ScheduledOn,
				StartedOn = run.StartedOn, Attempt = run.Attempt, ErrorCodeId = run.ErrorCodeId, FileId = run.FileId
			};
		}

		public RevenueFeedFile FindLatestFile(Guid runId) {
			return Runs[runId].File;
		}

		public RevenueFeedFile FindFile(Guid fileId) {
			return Files.TryGetValue(fileId, out RevenueFeedFile file) ? file : null;
		}

		public string FindRunWithSameFile(Guid runId, string fileHash) {
			return Runs.Values.FirstOrDefault(r => r.Id != runId && r.FileHash == fileHash
				&& (r.StatusId == RevenueFeedIds.RunStatus.Succeeded
					|| r.StatusId == RevenueFeedIds.RunStatus.CompletedWithErrors))?.Name;
		}

		public void UpdateRun(Guid runId, RevenueFeedRunUpdate update) {
			Run run = Runs[runId];
			run.StatusId = update.StatusId ?? run.StatusId;
			run.StageId = update.StageId ?? run.StageId;
			run.ScheduledOn = update.ScheduledOn ?? run.ScheduledOn;
			run.ReceivedOn = update.ReceivedOn ?? run.ReceivedOn;
			run.StartedOn = update.StartedOn ?? run.StartedOn;
			run.CompletedOn = update.CompletedOn ?? run.CompletedOn;
			run.Duration = update.Duration ?? run.Duration;
			run.LineCount = update.LineCount ?? run.LineCount;
			run.AppliedCount = update.AppliedCount ?? run.AppliedCount;
			run.RejectedCount = update.RejectedCount ?? run.RejectedCount;
			run.SkippedCount = update.SkippedCount ?? run.SkippedCount;
			run.Attempt = update.Attempt ?? run.Attempt;
			run.FileId = update.FileId ?? run.FileId;
			run.FileName = update.FileName ?? run.FileName;
			run.FileHash = update.FileHash ?? run.FileHash;
			if (update.ErrorCodeId.HasValue) {
				run.ErrorCodeId = update.ErrorCodeId == Guid.Empty ? (Guid?)null : update.ErrorCodeId;
			}
			run.ErrorMessage = update.ErrorMessage ?? run.ErrorMessage;
		}

		public void AddStep(RevenueFeedStepRecord step) {
			Steps.Add(step);
		}

		public IList<RevenueFeedLineRecord> GetLines(Guid runId) {
			return Lines.Values.Where(l => l.RunId == runId).Select(Copy).ToList();
		}

		public void SaveLine(RevenueFeedLineRecord line) {
			if (FailNextLineSave) {
				FailNextLineSave = false;
				throw new InvalidOperationException("Database is unavailable.");
			}
			if (FailLineSaveAfter == 0) {
				FailLineSaveAfter = -1;
				throw new InvalidOperationException("Database is unavailable.");
			}
			if (FailLineSaveAfter > 0) {
				FailLineSaveAfter--;
			}
			Lines[line.Id] = Copy(line);
		}

		public void DeleteLines(IEnumerable<Guid> lineIds) {
			foreach (Guid id in lineIds) {
				Lines.Remove(id);
			}
		}

		public IDictionary<string, Guid> FindAccountsByCode(ICollection<string> codes) {
			return Accounts.Where(a => codes.Contains(a.Key, StringComparer.OrdinalIgnoreCase))
				.ToDictionary(a => a.Key, a => a.Value, StringComparer.OrdinalIgnoreCase);
		}

		public IDictionary<string, RevenueFeedOpportunityInfo> FindOpportunitiesByCode(ICollection<string> codes) {
			return Opportunities.Where(o => codes.Contains(o.Code, StringComparer.OrdinalIgnoreCase))
				.ToDictionary(o => o.Code, o => new RevenueFeedOpportunityInfo { Id = o.Id, Code = o.Code, AccountId = o.AccountId },
					StringComparer.OrdinalIgnoreCase);
		}

		public string FindRunThatApplied(string billingReference, Guid excludingLineId) {
			RevenueFeedLineRecord line = Lines.Values.FirstOrDefault(l => l.Id != excludingLineId
				&& l.StatusId == RevenueFeedIds.LineStatus.Applied && l.BillingReference == billingReference);
			return line == null ? null : Runs[line.RunId].Name;
		}

		public decimal GetAppliedAmountForOpportunity(Guid opportunityId) {
			return Lines.Values.Where(l => l.OpportunityId == opportunityId && l.StatusId == RevenueFeedIds.LineStatus.Applied)
				.Sum(l => l.Amount.GetValueOrDefault());
		}

		public decimal GetAppliedAmountForAccount(Guid accountId) {
			return Lines.Values.Where(l => l.AccountId == accountId && l.StatusId == RevenueFeedIds.LineStatus.Applied)
				.Sum(l => l.Amount.GetValueOrDefault());
		}

		public void SetOpportunityActual(Guid opportunityId, decimal actualAmount, Guid runId) {
			if (FailOpportunityActual) {
				throw new InvalidOperationException("Saving Opportunity was cancelled by a business rule or listener.");
			}
			Opportunity opportunity = Opportunities.Single(o => o.Id == opportunityId);
			opportunity.ActualAmount = actualAmount;
			opportunity.Variance = actualAmount - opportunity.Amount;
			opportunity.VariancePercent = opportunity.Amount == 0 ? 0 : Math.Round(opportunity.Variance / opportunity.Amount * 100, 2);
			opportunity.LastRunId = runId;
		}

		public void SetAccountActual(Guid accountId, decimal actualAmount, Guid runId) {
			AccountActuals[accountId] = actualAmount;
		}

		private static RevenueFeedLineRecord Copy(RevenueFeedLineRecord l) {
			return (RevenueFeedLineRecord)typeof(object).GetMethod("MemberwiseClone",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(l, null);
		}
	}
}
