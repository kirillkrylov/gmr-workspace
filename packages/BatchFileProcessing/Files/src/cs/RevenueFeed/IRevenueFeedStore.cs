using System;
using System.Collections.Generic;

namespace BatchFileProcessingApp.RevenueFeed {

	internal sealed class RevenueFeedRunInfo {
		public Guid Id { get; set; }
		public string Name { get; set; }
		public Guid StatusId { get; set; }
		public DateTime? ScheduledOn { get; set; }
		public DateTime? StartedOn { get; set; }
		public int Attempt { get; set; }
		public Guid? ErrorCodeId { get; set; }
		public Guid? FileId { get; set; }
	}

	internal sealed class RevenueFeedFile {
		public Guid Id { get; set; }
		public string Name { get; set; }
		public byte[] Content { get; set; }
	}

	/// <summary>
	/// Changes to a run record. Only the properties that are set are written.
	/// </summary>
	internal sealed class RevenueFeedRunUpdate {
		public Guid? StatusId { get; set; }
		public Guid? StageId { get; set; }
		public DateTime? ScheduledOn { get; set; }
		public DateTime? ReceivedOn { get; set; }
		public DateTime? StartedOn { get; set; }
		public DateTime? CompletedOn { get; set; }
		public int? Duration { get; set; }
		public Guid? FileId { get; set; }
		public string FileName { get; set; }
		public string FileHash { get; set; }
		public int? LineCount { get; set; }
		public int? AppliedCount { get; set; }
		public int? RejectedCount { get; set; }
		public int? SkippedCount { get; set; }
		public int? Attempt { get; set; }
		/// <summary>Guid.Empty clears the error.</summary>
		public Guid? ErrorCodeId { get; set; }
		/// <summary>An empty string clears the message.</summary>
		public string ErrorMessage { get; set; }
	}

	internal sealed class RevenueFeedStepRecord {
		public Guid RunId { get; set; }
		public Guid StageId { get; set; }
		public string Name { get; set; }
		public Guid ResultId { get; set; }
		public DateTime StartedOn { get; set; }
		public DateTime CompletedOn { get; set; }
		public string Message { get; set; }
		public int Attempt { get; set; }
	}

	/// <summary>
	/// A RevenueFeedLine record. A line keeps its id across reprocessing, so its history stays on one record.
	/// </summary>
	internal sealed class RevenueFeedLineRecord {
		public Guid Id { get; set; }
		public Guid RunId { get; set; }
		public string Name { get; set; }
		public int LineNumber { get; set; }
		public string RawText { get; set; }
		public string BillingReference { get; set; }
		public string AccountCode { get; set; }
		public string OpportunityCode { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal? Amount { get; set; }
		public Guid StatusId { get; set; }
		public Guid? ErrorCodeId { get; set; }
		public string ErrorMessage { get; set; }
		public Guid? AccountId { get; set; }
		public Guid? OpportunityId { get; set; }
		public decimal? PreviousAmount { get; set; }
		public decimal? NewAmount { get; set; }
		public DateTime? AppliedOn { get; set; }
	}

	internal sealed class RevenueFeedOpportunityInfo {
		public Guid Id { get; set; }
		public string Code { get; set; }
		public Guid? AccountId { get; set; }
	}

	/// <summary>
	/// Data access for the revenue feed pipeline.
	/// </summary>
	internal interface IRevenueFeedStore {

		RevenueFeedRunInfo FindRun(Guid runId);

		/// <summary>The most recently uploaded, completely uploaded file of the run, or null when there is none.</summary>
		RevenueFeedFile FindLatestFile(Guid runId);

		/// <summary>An attached file by id, or null when it no longer exists.</summary>
		RevenueFeedFile FindFile(Guid fileId);

		/// <summary>The name of another run that already processed a file with this checksum, or null.</summary>
		string FindRunWithSameFile(Guid runId, string fileHash);

		void UpdateRun(Guid runId, RevenueFeedRunUpdate update);

		void AddStep(RevenueFeedStepRecord step);

		IList<RevenueFeedLineRecord> GetLines(Guid runId);

		/// <summary>Inserts the line when its id is new, otherwise overwrites the stored line.</summary>
		void SaveLine(RevenueFeedLineRecord line);

		void DeleteLines(IEnumerable<Guid> lineIds);

		/// <summary>Account ids by account code, for the codes that exist.</summary>
		IDictionary<string, Guid> FindAccountsByCode(ICollection<string> codes);

		/// <summary>Opportunities by revenue system code, for the codes that exist.</summary>
		IDictionary<string, RevenueFeedOpportunityInfo> FindOpportunitiesByCode(ICollection<string> codes);

		/// <summary>The run name of another applied line with this billing reference, in any run, or null.</summary>
		string FindRunThatApplied(string billingReference, Guid excludingLineId);

		/// <summary>Sum of applied line amounts of the opportunity.</summary>
		decimal GetAppliedAmountForOpportunity(Guid opportunityId);

		/// <summary>Sum of applied line amounts of the account.</summary>
		decimal GetAppliedAmountForAccount(Guid accountId);

		/// <summary>
		/// Sets actual revenue on the opportunity and recalculates its variance against the projected amount.
		/// Throws when the save is cancelled, so the stage fails instead of reporting totals it did not write.
		/// </summary>
		void SetOpportunityActual(Guid opportunityId, decimal actualAmount, Guid runId);

		void SetAccountActual(Guid accountId, decimal actualAmount, Guid runId);
	}
}
