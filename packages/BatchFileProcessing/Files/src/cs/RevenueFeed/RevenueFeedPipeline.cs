using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Common.Logging;
using ErrorOr;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// The outcome of one stage, returned to the business process that runs it.
	/// </summary>
	public sealed class RevenueFeedStageOutcome {
		public bool Succeeded { get; set; }
		public string Message { get; set; }
	}

	internal interface IRevenueFeedPipeline {

		/// <summary>
		/// Runs one stage of a run and records it as a step. Never throws: an unexpected error fails the stage and is
		/// recorded on the run as <see cref="RevenueFeedIds.ErrorCode.UnexpectedError"/>.
		/// </summary>
		RevenueFeedStageOutcome RunStage(Guid runId, Guid stageId);
	}

	internal interface IRevenueFeedNotifier {
		void NotifyProblem(Guid runId, string runName, Guid statusId, string message);
	}

	/// <summary>
	/// Serializes the stages that read and write shared revenue state (applied billing references and actual revenue
	/// totals) across runs and application nodes.
	/// </summary>
	internal interface IRevenueFeedLock {
		IDisposable Acquire();
	}

	internal sealed class RevenueFeedOptions {
		/// <summary>How often the revenue system is expected to deliver a file.</summary>
		public int IntervalMinutes { get; set; } = 120;

		/// <summary>
		/// Maps a time on the pipeline clock to the start of its delivery slot. Slots are computed in one feed time zone,
		/// so runs uploaded by users in different time zones and the missed-slot job agree on slot boundaries.
		/// </summary>
		public Func<DateTime, DateTime> GetSlot { get; set; }
	}

	/// <summary>
	/// The revenue feed stages. Each stage reads what the previous ones stored, so a stage can run in its own process
	/// element, and a run can be processed again: lines already applied are kept and never applied twice.
	/// </summary>
	internal sealed class RevenueFeedPipeline : IRevenueFeedPipeline {

		private sealed class StageResult {
			public Guid ResultId { get; private set; }
			public string Message { get; private set; }
			public Guid? ErrorCodeId { get; private set; }

			public bool Succeeded => ResultId != RevenueFeedIds.StepResult.Failed;

			public static StageResult Ok(string message) {
				return new StageResult { ResultId = RevenueFeedIds.StepResult.Succeeded, Message = message };
			}

			public static StageResult OkOrWarning(bool hasWarnings, string message) {
				return new StageResult {
					ResultId = hasWarnings ? RevenueFeedIds.StepResult.CompletedWithWarnings : RevenueFeedIds.StepResult.Succeeded,
					Message = message
				};
			}

			public static StageResult Fail(Guid errorCodeId, string message) {
				return new StageResult {
					ResultId = RevenueFeedIds.StepResult.Failed, Message = message, ErrorCodeId = errorCodeId
				};
			}
		}

		private static readonly IReadOnlyDictionary<Guid, string> StageNames = new Dictionary<Guid, string> {
			{ RevenueFeedIds.Stage.ReceiveFile, "Receive file" },
			{ RevenueFeedIds.Stage.ValidateFile, "Validate file" },
			{ RevenueFeedIds.Stage.ParseLines, "Parse lines" },
			{ RevenueFeedIds.Stage.MatchRecords, "Match records" },
			{ RevenueFeedIds.Stage.ApplyAmounts, "Apply amounts" },
			{ RevenueFeedIds.Stage.UpdateTotals, "Update totals" },
			{ RevenueFeedIds.Stage.CompleteRun, "Complete run" }
		};

		private static readonly IReadOnlyDictionary<Guid, string> ErrorNames = new Dictionary<Guid, string> {
			{ RevenueFeedIds.ErrorCode.MalformedLine, "Malformed line" },
			{ RevenueFeedIds.ErrorCode.MissingValue, "Missing value" },
			{ RevenueFeedIds.ErrorCode.InvalidNumber, "Invalid number" },
			{ RevenueFeedIds.ErrorCode.InvalidDate, "Invalid date" },
			{ RevenueFeedIds.ErrorCode.DuplicateInFile, "Duplicate in file" },
			{ RevenueFeedIds.ErrorCode.AccountNotFound, "Account not found" },
			{ RevenueFeedIds.ErrorCode.OpportunityNotFound, "Opportunity not found" },
			{ RevenueFeedIds.ErrorCode.OpportunityAccountMismatch, "Opportunity account mismatch" }
		};

		private readonly IRevenueFeedStore _store;
		private readonly IRevenueFeedNotifier _notifier;
		private readonly RevenueFeedOptions _options;
		private readonly IRevenueFeedLock _lock;
		private readonly Func<DateTime> _now;
		private readonly ILog _log;

		public RevenueFeedPipeline(IRevenueFeedStore store, IRevenueFeedNotifier notifier, RevenueFeedOptions options,
				IRevenueFeedLock feedLock, Func<DateTime> now, ILog log) {
			_store = store;
			_notifier = notifier;
			_options = options;
			_lock = feedLock;
			_now = now;
			_log = log;
		}

		public RevenueFeedStageOutcome RunStage(Guid runId, Guid stageId) {
			if (!StageNames.TryGetValue(stageId, out string stageName)) {
				return new RevenueFeedStageOutcome { Succeeded = false, Message = $"Unknown stage {stageId}." };
			}
			RevenueFeedRunInfo run = _store.FindRun(runId);
			if (run == null) {
				return new RevenueFeedStageOutcome { Succeeded = false, Message = $"Revenue feed run {runId} not found." };
			}
			DateTime startedOn = _now();
			StageResult result;
			try {
				if (stageId == RevenueFeedIds.Stage.ReceiveFile) {
					run = StartAttempt(run, startedOn);
				}
				_store.UpdateRun(runId, new RevenueFeedRunUpdate { StageId = stageId });
				result = ExecuteStage(run, stageId);
			} catch (Exception exception) {
				_log.Error($"Revenue feed run {run.Name}: stage \"{stageName}\" failed.", exception);
				result = StageResult.Fail(RevenueFeedIds.ErrorCode.UnexpectedError,
					$"{exception.GetType().Name}: {exception.Message}");
			}
			if (result.ErrorCodeId.HasValue) {
				_store.UpdateRun(runId, new RevenueFeedRunUpdate {
					ErrorCodeId = result.ErrorCodeId, ErrorMessage = $"{stageName}: {result.Message}"
				});
			}
			_store.AddStep(new RevenueFeedStepRecord {
				RunId = runId,
				StageId = stageId,
				Name = stageName,
				ResultId = result.ResultId,
				StartedOn = startedOn,
				CompletedOn = _now(),
				Message = result.Message,
				Attempt = run.Attempt
			});
			return new RevenueFeedStageOutcome { Succeeded = result.Succeeded, Message = result.Message };
		}

		/// <summary>
		/// The start of the slot a delivery time belongs to: slots are <see cref="RevenueFeedOptions.IntervalMinutes"/>
		/// long and start at midnight, in the time zone of the pipeline clock.
		/// </summary>
		internal static DateTime GetSlotStart(DateTime time, int intervalMinutes) {
			int interval = Math.Max(1, intervalMinutes);
			int minutesIntoDay = (int)time.TimeOfDay.TotalMinutes;
			return time.Date.AddMinutes(minutesIntoDay - minutesIntoDay % interval);
		}

		private RevenueFeedRunInfo StartAttempt(RevenueFeedRunInfo run, DateTime now) {
			run.Attempt++;
			run.StartedOn = now;
			run.ErrorCodeId = null;
			var update = new RevenueFeedRunUpdate {
				StatusId = RevenueFeedIds.RunStatus.Processing,
				StartedOn = now,
				Attempt = run.Attempt,
				ErrorCodeId = Guid.Empty,
				ErrorMessage = string.Empty
			};
			if (!run.ScheduledOn.HasValue) {
				run.ScheduledOn = _options.GetSlot != null ? _options.GetSlot(now) : GetSlotStart(now, _options.IntervalMinutes);
				update.ScheduledOn = run.ScheduledOn;
				update.ReceivedOn = now;
			}
			_store.UpdateRun(run.Id, update);
			return run;
		}

		private StageResult ExecuteStage(RevenueFeedRunInfo run, Guid stageId) {
			if (stageId == RevenueFeedIds.Stage.ReceiveFile) {
				return ReceiveFile(run);
			}
			if (stageId == RevenueFeedIds.Stage.ValidateFile) {
				return ValidateFile(run);
			}
			if (stageId == RevenueFeedIds.Stage.ParseLines) {
				return ParseLines(run);
			}
			if (stageId == RevenueFeedIds.Stage.MatchRecords) {
				return MatchRecords(run);
			}
			if (stageId == RevenueFeedIds.Stage.ApplyAmounts) {
				return ApplyAmounts(run);
			}
			if (stageId == RevenueFeedIds.Stage.UpdateTotals) {
				return UpdateTotals(run);
			}
			return CompleteRun(run);
		}

		private StageResult ReceiveFile(RevenueFeedRunInfo run) {
			RevenueFeedFile file = _store.FindLatestFile(run.Id);
			if (file == null) {
				return StageResult.Fail(RevenueFeedIds.ErrorCode.FileMissing,
					"No file is attached to the run. Upload the revenue CSV file to the run's attachments.");
			}
			string hash = ComputeHash(file.Content);
			run.FileId = file.Id;
			_store.UpdateRun(run.Id, new RevenueFeedRunUpdate { FileId = file.Id, FileName = file.Name, FileHash = hash });
			string otherRun = _store.FindRunWithSameFile(run.Id, hash);
			if (otherRun != null) {
				return StageResult.Fail(RevenueFeedIds.ErrorCode.DuplicateFile,
					$"\"{file.Name}\" has the same content as the file already processed by run {otherRun}.");
			}
			return StageResult.Ok($"Received \"{file.Name}\" ({file.Content?.Length ?? 0} bytes).");
		}

		private StageResult ValidateFile(RevenueFeedRunInfo run) {
			ErrorOr<IReadOnlyList<KeyValuePair<int, string>>> dataLines = ReadDataLines(run);
			if (dataLines.IsError) {
				return StageResult.Fail(RevenueFeedErrors.ToErrorCodeId(dataLines.FirstError.Code),
					dataLines.FirstError.Description);
			}
			return StageResult.Ok($"The file is readable CSV with the expected header and {dataLines.Value.Count} data lines.");
		}

		private StageResult ParseLines(RevenueFeedRunInfo run) {
			ErrorOr<IReadOnlyList<KeyValuePair<int, string>>> dataLines = ReadDataLines(run);
			if (dataLines.IsError) {
				return StageResult.Fail(RevenueFeedErrors.ToErrorCodeId(dataLines.FirstError.Code),
					dataLines.FirstError.Description);
			}
			IReadOnlyList<ParsedRevenueLine> parsed = RevenueCsvParser.ParseLines(dataLines.Value);
			IList<RevenueFeedLineRecord> existing = _store.GetLines(run.Id);
			// Applied lines are the ledger: they stay as they are, whatever line their reference sits on in this file.
			var appliedReferences = new HashSet<string>(existing
				.Where(l => l.StatusId == RevenueFeedIds.LineStatus.Applied).Select(l => l.BillingReference),
				StringComparer.OrdinalIgnoreCase);
			Dictionary<int, RevenueFeedLineRecord> reusable = existing
				.Where(l => l.StatusId != RevenueFeedIds.LineStatus.Applied)
				.GroupBy(l => l.LineNumber).ToDictionary(g => g.Key, g => g.First());
			var reusedIds = new HashSet<Guid>();
			int kept = 0;
			foreach (ParsedRevenueLine line in parsed) {
				if (line.IsValid && appliedReferences.Contains(line.BillingReference)) {
					kept++;
					continue;
				}
				Guid id = reusable.TryGetValue(line.LineNumber, out RevenueFeedLineRecord record) ? record.Id : Guid.NewGuid();
				reusedIds.Add(id);
				_store.SaveLine(ToRecord(run, line, id));
			}
			_store.DeleteLines(reusable.Values.Where(l => !reusedIds.Contains(l.Id)).Select(l => l.Id).ToList());
			int rejected = parsed.Count(l => !l.IsValid);
			_store.UpdateRun(run.Id, new RevenueFeedRunUpdate { LineCount = parsed.Count });
			string message = $"{parsed.Count} lines: {parsed.Count - rejected - kept} to process, {rejected} rejected";
			if (kept > 0) {
				message += $", {kept} already applied by an earlier attempt";
			}
			return StageResult.OkOrWarning(rejected > 0, message + DescribeErrors(parsed.Where(l => !l.IsValid)
				.Select(l => l.ErrorCodeId.Value)) + ".");
		}

		private StageResult MatchRecords(RevenueFeedRunInfo run) {
			List<RevenueFeedLineRecord> pending = GetPendingLines(run.Id);
			IDictionary<string, Guid> accounts = _store.FindAccountsByCode(
				pending.Select(l => l.AccountCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
			IDictionary<string, RevenueFeedOpportunityInfo> opportunities = _store.FindOpportunitiesByCode(
				pending.Where(l => !string.IsNullOrEmpty(l.OpportunityCode)).Select(l => l.OpportunityCode)
					.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
			var rejectedCodes = new List<Guid>();
			foreach (RevenueFeedLineRecord line in pending) {
				if (!TryGet(accounts, line.AccountCode, out Guid accountId)) {
					Reject(line, RevenueFeedIds.ErrorCode.AccountNotFound, $"No account has code \"{line.AccountCode}\".");
				} else if (string.IsNullOrEmpty(line.OpportunityCode)) {
					line.AccountId = accountId;
				} else if (!TryGet(opportunities, line.OpportunityCode, out RevenueFeedOpportunityInfo opportunity)) {
					line.AccountId = accountId;
					Reject(line, RevenueFeedIds.ErrorCode.OpportunityNotFound,
						$"No opportunity has revenue system code \"{line.OpportunityCode}\".");
				} else if (opportunity.AccountId != accountId) {
					line.AccountId = accountId;
					line.OpportunityId = opportunity.Id;
					Reject(line, RevenueFeedIds.ErrorCode.OpportunityAccountMismatch,
						$"Opportunity \"{line.OpportunityCode}\" does not belong to account \"{line.AccountCode}\".");
				} else {
					line.AccountId = accountId;
					line.OpportunityId = opportunity.Id;
				}
				if (line.ErrorCodeId.HasValue) {
					rejectedCodes.Add(line.ErrorCodeId.Value);
				}
				_store.SaveLine(line);
			}
			return StageResult.OkOrWarning(rejectedCodes.Count > 0,
				$"{pending.Count - rejectedCodes.Count} lines matched, {rejectedCodes.Count} rejected" +
				DescribeErrors(rejectedCodes) + ".");
		}

		private StageResult ApplyAmounts(RevenueFeedRunInfo run) {
			using (_lock.Acquire()) {
				return ApplyAmountsLocked(run);
			}
		}

		private StageResult ApplyAmountsLocked(RevenueFeedRunInfo run) {
			List<RevenueFeedLineRecord> pending = GetPendingLines(run.Id);
			var opportunityTotals = new Dictionary<Guid, decimal>();
			var accountTotals = new Dictionary<Guid, decimal>();
			int applied = 0;
			int skipped = 0;
			foreach (RevenueFeedLineRecord line in pending.OrderBy(l => l.LineNumber)) {
				string otherRun = _store.FindRunThatApplied(line.BillingReference, line.Id);
				if (otherRun != null) {
					line.StatusId = RevenueFeedIds.LineStatus.Skipped;
					line.ErrorCodeId = RevenueFeedIds.ErrorCode.AlreadyApplied;
					line.ErrorMessage = $"Billing reference \"{line.BillingReference}\" was already applied by run {otherRun}.";
					skipped++;
				} else {
					// Before/after is the opportunity's actual revenue, or the account's for an account-level line.
					decimal amount = line.Amount.GetValueOrDefault();
					decimal accountPrevious = CurrentTotal(accountTotals, line.AccountId.Value, _store.GetAppliedAmountForAccount);
					accountTotals[line.AccountId.Value] = accountPrevious + amount;
					if (line.OpportunityId.HasValue) {
						decimal previous = CurrentTotal(opportunityTotals, line.OpportunityId.Value,
							_store.GetAppliedAmountForOpportunity);
						opportunityTotals[line.OpportunityId.Value] = previous + amount;
						line.PreviousAmount = previous;
						line.NewAmount = previous + amount;
					} else {
						line.PreviousAmount = accountPrevious;
						line.NewAmount = accountPrevious + amount;
					}
					line.StatusId = RevenueFeedIds.LineStatus.Applied;
					line.AppliedOn = _now();
					applied++;
				}
				_store.SaveLine(line);
			}
			return StageResult.Ok($"{applied} lines applied, {skipped} skipped as already applied.");
		}

		private StageResult UpdateTotals(RevenueFeedRunInfo run) {
			return StageResult.Ok(RecalculateTotals(run.Id));
		}

		/// <summary>
		/// Recalculates actual revenue from the applied lines for every account and opportunity this run applied to.
		/// Sums are read and written under the feed lock, so a concurrent run cannot overwrite a newer total.
		/// </summary>
		private string RecalculateTotals(Guid runId) {
			using (_lock.Acquire()) {
				List<RevenueFeedLineRecord> applied = _store.GetLines(runId)
					.Where(l => l.StatusId == RevenueFeedIds.LineStatus.Applied).ToList();
				List<Guid> opportunityIds = applied.Where(l => l.OpportunityId.HasValue)
					.Select(l => l.OpportunityId.Value).Distinct().ToList();
				List<Guid> accountIds = applied.Where(l => l.AccountId.HasValue)
					.Select(l => l.AccountId.Value).Distinct().ToList();
				foreach (Guid opportunityId in opportunityIds) {
					_store.SetOpportunityActual(opportunityId, _store.GetAppliedAmountForOpportunity(opportunityId), runId);
				}
				foreach (Guid accountId in accountIds) {
					_store.SetAccountActual(accountId, _store.GetAppliedAmountForAccount(accountId), runId);
				}
				return $"Actual revenue recalculated for {opportunityIds.Count} opportunities and {accountIds.Count} accounts.";
			}
		}

		private StageResult CompleteRun(RevenueFeedRunInfo run) {
			RevenueFeedRunInfo current = _store.FindRun(run.Id) ?? run;
			IList<RevenueFeedLineRecord> lines = _store.GetLines(run.Id);
			int applied = lines.Count(l => l.StatusId == RevenueFeedIds.LineStatus.Applied);
			int rejected = lines.Count(l => l.StatusId == RevenueFeedIds.LineStatus.Rejected);
			int skipped = lines.Count(l => l.StatusId == RevenueFeedIds.LineStatus.Skipped);
			Guid statusId;
			string message;
			if (current.ErrorCodeId == RevenueFeedIds.ErrorCode.DuplicateFile) {
				statusId = RevenueFeedIds.RunStatus.Skipped;
				message = "Skipped: this file was already processed. Nothing was applied.";
			} else if (current.ErrorCodeId.HasValue) {
				statusId = RevenueFeedIds.RunStatus.Failed;
				message = "Failed: the file was rejected. See the run error and steps.";
				if (applied > 0) {
					// A stage failed after lines were applied: keep actual revenue consistent with the applied lines.
					// The run must be finalized even when that recalculation fails too.
					string totals;
					try {
						totals = RecalculateTotals(run.Id);
					} catch (Exception exception) {
						_log.Error($"Revenue feed run {current.Name}: recalculating totals after a failure failed.", exception);
						totals = $"Actual revenue could not be recalculated ({exception.GetType().Name}: {exception.Message}).";
					}
					message = $"Failed after {applied} lines were applied. {totals} See the run error and steps.";
				}
			} else if (rejected > 0) {
				statusId = RevenueFeedIds.RunStatus.CompletedWithErrors;
				message = $"Completed with errors: {applied} applied, {rejected} rejected" +
					DescribeErrors(lines.Where(l => l.StatusId == RevenueFeedIds.LineStatus.Rejected && l.ErrorCodeId.HasValue)
						.Select(l => l.ErrorCodeId.Value)) + $", {skipped} already applied.";
			} else {
				statusId = RevenueFeedIds.RunStatus.Succeeded;
				message = $"Succeeded: {applied} applied, {skipped} already applied.";
			}
			DateTime completedOn = _now();
			var update = new RevenueFeedRunUpdate {
				StatusId = statusId,
				CompletedOn = completedOn,
				Duration = current.StartedOn.HasValue ? (int)Math.Round((completedOn - current.StartedOn.Value).TotalSeconds) : 0,
				AppliedCount = applied,
				RejectedCount = rejected,
				SkippedCount = skipped
			};
			if (statusId == RevenueFeedIds.RunStatus.CompletedWithErrors) {
				update.ErrorMessage = message;
			}
			_store.UpdateRun(run.Id, update);
			if (statusId == RevenueFeedIds.RunStatus.Failed || statusId == RevenueFeedIds.RunStatus.CompletedWithErrors) {
				_notifier.NotifyProblem(run.Id, current.Name, statusId, message);
			}
			return StageResult.Ok(message);
		}

		private ErrorOr<IReadOnlyList<KeyValuePair<int, string>>> ReadDataLines(RevenueFeedRunInfo run) {
			// Every stage of an attempt reads the file Receive file selected, even if another one is uploaded meanwhile.
			RevenueFeedFile file = run.FileId.HasValue ? _store.FindFile(run.FileId.Value) : null;
			if (file == null) {
				return Error.Validation(RevenueFeedErrors.FileMissing, "The file received for this attempt is no longer attached.");
			}
			return RevenueCsvParser.ReadDataLines(file.Content);
		}

		private List<RevenueFeedLineRecord> GetPendingLines(Guid runId) {
			return _store.GetLines(runId).Where(l => l.StatusId == RevenueFeedIds.LineStatus.Pending).ToList();
		}

		private static RevenueFeedLineRecord ToRecord(RevenueFeedRunInfo run, ParsedRevenueLine line, Guid id) {
			return new RevenueFeedLineRecord {
				Id = id,
				RunId = run.Id,
				Name = $"{run.Name} / {line.LineNumber}",
				LineNumber = line.LineNumber,
				RawText = line.RawText,
				BillingReference = line.BillingReference,
				AccountCode = line.AccountCode,
				OpportunityCode = line.OpportunityCode,
				StartDate = line.StartDate,
				EndDate = line.EndDate,
				Amount = line.Amount,
				StatusId = line.IsValid ? RevenueFeedIds.LineStatus.Pending : RevenueFeedIds.LineStatus.Rejected,
				ErrorCodeId = line.ErrorCodeId,
				ErrorMessage = line.ErrorMessage
			};
		}

		private static void Reject(RevenueFeedLineRecord line, Guid errorCodeId, string message) {
			line.StatusId = RevenueFeedIds.LineStatus.Rejected;
			line.ErrorCodeId = errorCodeId;
			line.ErrorMessage = message;
		}

		private static bool TryGet<T>(IDictionary<string, T> map, string key, out T value) {
			foreach (KeyValuePair<string, T> pair in map) {
				if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase)) {
					value = pair.Value;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		private static decimal CurrentTotal(Dictionary<Guid, decimal> totals, Guid id, Func<Guid, decimal> load) {
			if (!totals.TryGetValue(id, out decimal total)) {
				total = load(id);
				totals[id] = total;
			}
			return total;
		}

		private static string DescribeErrors(IEnumerable<Guid> errorCodeIds) {
			List<string> parts = errorCodeIds.GroupBy(id => id)
				.OrderByDescending(g => g.Count())
				.Select(g => $"{(ErrorNames.TryGetValue(g.Key, out string name) ? name : "Other")} ({g.Count()})")
				.ToList();
			return parts.Count == 0 ? string.Empty : $" ({string.Join(", ", parts)})";
		}

		private static string ComputeHash(byte[] content) {
			using (SHA256 sha = SHA256.Create()) {
				return BitConverter.ToString(sha.ComputeHash(content ?? new byte[0])).Replace("-", string.Empty);
			}
		}
	}
}
