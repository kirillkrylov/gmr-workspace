using System;
using System.Linq;
using System.Text;
using BatchFileProcessingApp.RevenueFeed;
using Common.Logging;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BatchFileProcessing.Tests.RevenueFeed {

	[TestFixture(Category = "UnitTests")]
	public class RevenueFeedPipelineTests {

		private const string Header = "BillingReference,AccountCode,OpportunityCode,PeriodStart,PeriodEnd,Amount";

		private static readonly Guid[] AllStages = {
			RevenueFeedIds.Stage.ReceiveFile, RevenueFeedIds.Stage.ValidateFile, RevenueFeedIds.Stage.ParseLines,
			RevenueFeedIds.Stage.MatchRecords, RevenueFeedIds.Stage.ApplyAmounts, RevenueFeedIds.Stage.UpdateTotals,
			RevenueFeedIds.Stage.CompleteRun
		};

		private InMemoryRevenueFeedStore _store;
		private IRevenueFeedNotifier _notifier;
		private RevenueFeedPipeline _pipeline;
		private DateTime _now;
		private Guid _accountId;
		private Guid _otherAccountId;
		private InMemoryRevenueFeedStore.Opportunity _opportunity;
		private CountingLock _feedLock;

		private sealed class CountingLock : IRevenueFeedLock {
			public int Held;
			public int Acquisitions;

			public IDisposable Acquire() {
				Held++;
				Acquisitions++;
				return new Releaser(this);
			}

			private sealed class Releaser : IDisposable {
				private readonly CountingLock _owner;
				public Releaser(CountingLock owner) { _owner = owner; }
				public void Dispose() { _owner.Held--; }
			}
		}

		[SetUp]
		public void SetUp() {
			_store = new InMemoryRevenueFeedStore();
			_notifier = Substitute.For<IRevenueFeedNotifier>();
			_now = new DateTime(2026, 10, 27, 10, 17, 0);
			_feedLock = new CountingLock();
			_pipeline = new RevenueFeedPipeline(_store, _notifier, new RevenueFeedOptions { IntervalMinutes = 120 },
				_feedLock, () => _now, Substitute.For<ILog>());
			_accountId = Guid.NewGuid();
			_otherAccountId = Guid.NewGuid();
			_store.Accounts.Add("ACC-1", _accountId);
			_store.Accounts.Add("ACC-2", _otherAccountId);
			_opportunity = new InMemoryRevenueFeedStore.Opportunity {
				Id = Guid.NewGuid(), Code = "OPP-1", AccountId = _accountId, Amount = 1000m
			};
			_store.Opportunities.Add(_opportunity);
		}

		private static RevenueFeedFile File(params string[] lines) {
			return new RevenueFeedFile { Name = "billing.csv", Content = Encoding.UTF8.GetBytes(string.Join("\n", lines)) };
		}

		/// <summary>Runs stages like the business process does: on a failed stage it jumps to Complete run.</summary>
		private void Process(Guid runId) {
			foreach (Guid stage in AllStages) {
				RevenueFeedStageOutcome outcome = _pipeline.RunStage(runId, stage);
				if (!outcome.Succeeded) {
					_pipeline.RunStage(runId, RevenueFeedIds.Stage.CompleteRun);
					return;
				}
			}
		}

		[Test]
		public void CleanFile_Succeeds_AndUpdatesActualsAndVariance() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000001", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700",
				"INV-2,ACC-1,OPP-1,2026-10-01,2026-10-31,500",
				"INV-3,ACC-1,,2026-10-01,2026-10-31,50"));

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Succeeded);
			run.AppliedCount.Should().Be(3);
			run.Attempt.Should().Be(1);
			run.ScheduledOn.Should().Be(new DateTime(2026, 10, 27, 10, 0, 0));
			_opportunity.ActualAmount.Should().Be(1200m);
			_opportunity.Variance.Should().Be(200m);
			_opportunity.VariancePercent.Should().Be(20m);
			_store.AccountActuals[_accountId].Should().Be(1250m);
			RevenueFeedLineRecord second = _store.Lines.Values.Single(l => l.BillingReference == "INV-2");
			second.PreviousAmount.Should().Be(700m);
			second.NewAmount.Should().Be(1200m);
			RevenueFeedLineRecord accountLine = _store.Lines.Values.Single(l => l.BillingReference == "INV-3");
			accountLine.PreviousAmount.Should().Be(1200m);
			accountLine.NewAmount.Should().Be(1250m);
			_store.Steps.Select(s => s.StageId).Should().Equal(AllStages);
			_store.Steps.Should().OnlyContain(s => s.ResultId == RevenueFeedIds.StepResult.Succeeded);
			_notifier.DidNotReceiveWithAnyArgs().NotifyProblem(default(Guid), null, default(Guid), null);
		}

		[Test]
		public void LineErrors_CompleteWithErrors_ApplyTheRest_AndNotify() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000002", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700",
				"INV-2,ACC-9,,2026-09-01,2026-09-30,10",
				"INV-3,ACC-2,OPP-1,2026-09-01,2026-09-30,10",
				"INV-4,ACC-1,OPP-9,2026-09-01,2026-09-30,10",
				"INV-5,ACC-1,,2026-09-01,2026-09-30,abc"));

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.CompletedWithErrors);
			run.AppliedCount.Should().Be(1);
			run.RejectedCount.Should().Be(4);
			run.ErrorCodeId.Should().BeNull();
			run.ErrorMessage.Should().Contain("Account not found (1)").And.Contain("Invalid number (1)");
			Line("INV-2").ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.AccountNotFound);
			Line("INV-2").ErrorMessage.Should().Contain("ACC-9");
			Line("INV-3").ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.OpportunityAccountMismatch);
			Line("INV-4").ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.OpportunityNotFound);
			Line("INV-5").ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.InvalidNumber);
			_opportunity.ActualAmount.Should().Be(700m);
			_store.Steps.Single(s => s.StageId == RevenueFeedIds.Stage.MatchRecords).ResultId
				.Should().Be(RevenueFeedIds.StepResult.CompletedWithWarnings);
			_notifier.Received(1).NotifyProblem(run.Id, "RF-000002", RevenueFeedIds.RunStatus.CompletedWithErrors,
				Arg.Any<string>());
		}

		[Test]
		public void CorruptFile_FailsAtValidate_AndAppliesNothing() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000003",
				new RevenueFeedFile { Name = "billing.csv", Content = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF } });

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Failed);
			run.ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.FileUnreadable);
			run.ErrorMessage.Should().StartWith("Validate file:");
			run.StageId.Should().Be(RevenueFeedIds.Stage.CompleteRun);
			_store.Steps.Select(s => s.StageId).Should().Equal(RevenueFeedIds.Stage.ReceiveFile,
				RevenueFeedIds.Stage.ValidateFile, RevenueFeedIds.Stage.CompleteRun);
			_store.Steps[1].ResultId.Should().Be(RevenueFeedIds.StepResult.Failed);
			_store.Lines.Should().BeEmpty();
			_notifier.Received(1).NotifyProblem(run.Id, "RF-000003", RevenueFeedIds.RunStatus.Failed, Arg.Any<string>());
		}

		[Test]
		public void NoFile_FailsAtReceive() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000004");

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Failed);
			run.ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.FileMissing);
		}

		[Test]
		public void SameFileAgain_IsSkipped_WithoutNotification() {
			RevenueFeedFile file = File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700");
			InMemoryRevenueFeedStore.Run first = _store.AddRun("RF-000005", file);
			InMemoryRevenueFeedStore.Run second = _store.AddRun("RF-000006", file);

			Process(first.Id);
			Process(second.Id);

			second.StatusId.Should().Be(RevenueFeedIds.RunStatus.Skipped);
			second.ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.DuplicateFile);
			second.ErrorMessage.Should().Contain("RF-000005");
			_opportunity.ActualAmount.Should().Be(700m);
			_notifier.DidNotReceiveWithAnyArgs().NotifyProblem(default(Guid), null, default(Guid), null);
		}

		[Test]
		public void BillingReferenceInAnotherFile_IsSkippedAsAlreadyApplied() {
			InMemoryRevenueFeedStore.Run first = _store.AddRun("RF-000007",
				File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			InMemoryRevenueFeedStore.Run second = _store.AddRun("RF-000008", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700",
				"INV-2,ACC-1,OPP-1,2026-10-01,2026-10-31,100"));

			Process(first.Id);
			Process(second.Id);

			second.StatusId.Should().Be(RevenueFeedIds.RunStatus.Succeeded);
			second.SkippedCount.Should().Be(1);
			_store.Lines.Values.Single(l => l.RunId == second.Id && l.BillingReference == "INV-1").ErrorMessage
				.Should().Contain("RF-000007");
			_opportunity.ActualAmount.Should().Be(800m);
		}

		[Test]
		public void Reprocess_AfterFixingAccount_AppliesOnlyRejectedLines_AndKeepsLineIds() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000009", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700",
				"INV-2,ACC-3,,2026-09-01,2026-09-30,40"));
			Process(run.Id);
			Guid rejectedLineId = Line("INV-2").Id;
			Guid appliedLineId = Line("INV-1").Id;
			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.CompletedWithErrors);

			Guid newAccountId = Guid.NewGuid();
			_store.Accounts.Add("ACC-3", newAccountId);
			_now = _now.AddMinutes(30);
			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Succeeded);
			run.Attempt.Should().Be(2);
			run.AppliedCount.Should().Be(2);
			run.ErrorCodeId.Should().BeNull();
			Line("INV-2").Id.Should().Be(rejectedLineId);
			Line("INV-2").StatusId.Should().Be(RevenueFeedIds.LineStatus.Applied);
			Line("INV-1").Id.Should().Be(appliedLineId);
			_opportunity.ActualAmount.Should().Be(700m);
			_store.AccountActuals[newAccountId].Should().Be(40m);
			_store.Steps.Where(s => s.Attempt == 2).Should().HaveCount(AllStages.Length);
			run.ScheduledOn.Should().Be(new DateTime(2026, 10, 27, 10, 0, 0), "a reprocess keeps the original slot");
		}

		[Test]
		public void UnexpectedException_FailsStage_WithUnexpectedError() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000010",
				File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			_store.FailNextLineSave = true;

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Failed);
			run.ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.UnexpectedError);
			run.ErrorMessage.Should().Contain("Parse lines").And.Contain("Database is unavailable");
			_store.Steps.Single(s => s.StageId == RevenueFeedIds.Stage.ParseLines).ResultId
				.Should().Be(RevenueFeedIds.StepResult.Failed);
		}

		[Test]
		public void UnknownRun_FailsWithoutThrowing() {
			RevenueFeedStageOutcome outcome = _pipeline.RunStage(Guid.NewGuid(), RevenueFeedIds.Stage.ReceiveFile);

			outcome.Succeeded.Should().BeFalse();
			_store.Steps.Should().BeEmpty();
		}

		[TestCase(0, 0, 0)]
		[TestCase(1, 59, 0)]
		[TestCase(2, 0, 2)]
		[TestCase(23, 59, 22)]
		public void GetSlotStart_AlignsToIntervalFromMidnight(int hour, int minute, int expectedHour) {
			RevenueFeedPipeline.GetSlotStart(new DateTime(2026, 10, 27, hour, minute, 30), 120)
				.Should().Be(new DateTime(2026, 10, 27, expectedHour, 0, 0));
		}

		[Test]
		public void MissedRunJob_ChecksTheSlotWhoseGracePeriodEnded() {
			RevenueFeedMissedRunJob.GetSlotToCheck(new DateTime(2026, 10, 27, 12, 14, 0), 120, 15)
				.Should().Be(new DateTime(2026, 10, 27, 8, 0, 0));
			RevenueFeedMissedRunJob.GetSlotToCheck(new DateTime(2026, 10, 27, 12, 15, 0), 120, 15)
				.Should().Be(new DateTime(2026, 10, 27, 10, 0, 0));
		}

		[Test]
		public void StageForElement_MapsEveryProcessElementName() {
			new[] { "ReceiveFile", "ValidateFile", "ParseLines", "MatchRecords", "ApplyAmounts", "UpdateTotals", "CompleteRun" }
				.Select(RevenueFeedIds.StageForElement).Should().Equal(AllStages.Cast<Guid?>());
			RevenueFeedIds.StageForElement("Other").Should().BeNull();
		}

		[Test]
		public void ReplacementFileWithShiftedLines_NeverAppliesAReferenceTwice() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000011", File(Header,
				"INV-1,ACC-9,,2026-09-01,2026-09-30,40",
				"INV-2,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			Process(run.Id);
			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.CompletedWithErrors);

			// The corrected file drops the bad line, so INV-2 moves from line 3 to line 2.
			_store.Attach(run, File(Header, "INV-2,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			run.StatusId = RevenueFeedIds.RunStatus.Failed;
			Process(run.Id);

			_store.Lines.Values.Count(l => l.BillingReference == "INV-2" && l.StatusId == RevenueFeedIds.LineStatus.Applied)
				.Should().Be(1);
			_store.Lines.Values.Should().NotContain(l => l.BillingReference == "INV-1");
			_opportunity.ActualAmount.Should().Be(700m);
			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Succeeded);
		}

		[Test]
		public void StagesOfOneAttempt_ReadTheFileReceived_EvenIfAnotherIsUploaded() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000012",
				File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.ReceiveFile);
			_store.Attach(run, File(Header, "INV-9,ACC-1,OPP-1,2026-09-01,2026-09-30,1"));

			foreach (Guid stage in AllStages.Skip(1)) {
				_pipeline.RunStage(run.Id, stage);
			}

			_store.Lines.Values.Select(l => l.BillingReference).Should().Equal("INV-1");
			_opportunity.ActualAmount.Should().Be(700m);
		}

		[Test]
		public void ApplyAndTotals_RunUnderTheFeedLock() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000013",
				File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));

			Process(run.Id);

			_feedLock.Acquisitions.Should().Be(2, "Apply amounts and Update totals each take the lock");
			_feedLock.Held.Should().Be(0);
		}

		[Test]
		public void FailureAfterSomeLinesApplied_RecalculatesTotalsForTheAppliedLines() {
			var otherOpportunity = new InMemoryRevenueFeedStore.Opportunity {
				Id = Guid.NewGuid(), Code = "OPP-2", AccountId = _otherAccountId, Amount = 100m
			};
			_store.Opportunities.Add(otherOpportunity);
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000014", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700",
				"INV-2,ACC-2,OPP-2,2026-09-01,2026-09-30,50"));
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.ReceiveFile);
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.ValidateFile);
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.ParseLines);
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.MatchRecords);
			_store.FailLineSaveAfter = 1;

			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.ApplyAmounts).Succeeded.Should().BeFalse();
			_pipeline.RunStage(run.Id, RevenueFeedIds.Stage.CompleteRun);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Failed);
			_opportunity.ActualAmount.Should().Be(700m, "the applied line's revenue is posted although the run failed");
			_store.Steps.Last().Message.Should().Contain("Failed after 1 lines were applied");
		}

		[Test]
		public void TotalsThatCannotBeSaved_StillFinalizeTheRunAsFailed() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000016",
				File(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,700"));
			_store.FailOpportunityActual = true;

			Process(run.Id);

			run.StatusId.Should().Be(RevenueFeedIds.RunStatus.Failed);
			run.ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.UnexpectedError);
			run.ErrorMessage.Should().StartWith("Update totals:");
			_store.Steps.Last().StageId.Should().Be(RevenueFeedIds.Stage.CompleteRun);
			_store.Steps.Last().Message.Should().Contain("could not be recalculated");
			_notifier.Received(1).NotifyProblem(run.Id, "RF-000016", RevenueFeedIds.RunStatus.Failed, Arg.Any<string>());
		}

		[Test]
		public void UnterminatedQuote_RejectsTheLineAsMalformed() {
			InMemoryRevenueFeedStore.Run run = _store.AddRun("RF-000015", File(Header,
				"INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,\"100"));

			Process(run.Id);

			_store.Lines.Values.Single(l => l.LineNumber == 2).ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.MalformedLine);
			_opportunity.ActualAmount.Should().Be(0m);
		}

		[Test]
		public void MissedSlots_BackfillsOverdueSlotsSinceTheFirstRun() {
			var latest = new DateTime(2026, 10, 27, 10, 0, 0);
			var known = new[] { new DateTime(2026, 10, 27, 4, 0, 0), new DateTime(2026, 10, 27, 8, 0, 0) };

			RevenueFeedMissedRunJob.GetMissedSlots(latest, 120, known, new DateTime(2026, 10, 27, 4, 0, 0))
				.Should().Equal(new DateTime(2026, 10, 27, 6, 0, 0), new DateTime(2026, 10, 27, 10, 0, 0));
		}

		[Test]
		public void MissedSlots_WithoutRuns_ChecksOnlyTheLatestSlot() {
			var latest = new DateTime(2026, 10, 27, 10, 0, 0);

			RevenueFeedMissedRunJob.GetMissedSlots(latest, 120, new DateTime[0], null).Should().Equal(latest);
		}

		[Test]
		public void MissedSlots_BackfillIsCappedAtADay() {
			var latest = new DateTime(2026, 10, 27, 10, 0, 0);

			RevenueFeedMissedRunJob.GetMissedSlots(latest, 120, new DateTime[0], new DateTime(2026, 10, 20, 0, 0, 0))
				.Should().HaveCount(13).And.StartWith(new DateTime(2026, 10, 26, 10, 0, 0));
		}

		[Test]
		public void GetSlot_AlignsToTheFeedTimeZoneForEveryUser() {
			TimeZoneInfo feed = TimeZoneInfo.CreateCustomTimeZone("Feed", TimeSpan.FromHours(-5), "Feed", "Feed");
			TimeZoneInfo user = TimeZoneInfo.CreateCustomTimeZone("User", TimeSpan.FromHours(1), "User", "User");

			// 16:30 for a UTC+1 user is 10:30 in the UTC-5 feed zone: slot 10:00 feed time = 16:00 user time.
			RevenueFeedService.GetSlot(new DateTime(2026, 10, 27, 16, 30, 0), user, feed, 120)
				.Should().Be(new DateTime(2026, 10, 27, 16, 0, 0));
			RevenueFeedService.GetSlot(new DateTime(2026, 10, 27, 10, 30, 0), feed, feed, 120)
				.Should().Be(new DateTime(2026, 10, 27, 10, 0, 0));
		}

		[TestCase(0, 0, true)]
		[TestCase(100, 100, true)]
		[TestCase(50, 100, false)]
		[TestCase(100, 0, true)]
		public void IsUploadComplete_WaitsForTheLastChunk(long size, long totalSize, bool expected) {
			RevenueFeedStore.IsUploadComplete(size, totalSize).Should().Be(expected);
		}

		private RevenueFeedLineRecord Line(string billingReference) {
			return _store.Lines.Values.Single(l => l.BillingReference == billingReference);
		}
	}
}
