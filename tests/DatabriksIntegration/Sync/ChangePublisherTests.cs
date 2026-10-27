using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Logging;
using Databricks.Zerobus;
using DatabriksIntegrationApp.Sync;
using DatabriksIntegrationApp.Zerobus;
using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Terrasoft.Core;

namespace DatabriksIntegration.Tests.Sync {

	[TestFixture(Category = "UnitTests")]
	public class ChangePublisherTests {
		private const string Table = "workspace.default.creatio_changes";
		private IDatabricksOptionsProvider _optionsProvider;
		private IChangeStreamFactory _streamFactory;
		private IChangeStream _stream;
		private ILog _log;
		private DatabricksOptions _options;
		private List<TimeSpan> _delays;
		private ChangePublisher _publisher;

		[SetUp]
		public void SetUp() {
			_options = new DatabricksOptions {
				ZerobusEndpoint = "https://1.zerobus.example.com",
				WorkspaceUrl = "https://dbc.example.com",
				ClientId = "client",
				ClientSecret = "secret"
			};
			_optionsProvider = Substitute.For<IDatabricksOptionsProvider>();
			_optionsProvider.Get(Arg.Any<UserConnection>()).Returns(x => _options);
			_stream = Substitute.For<IChangeStream>();
			_streamFactory = Substitute.For<IChangeStreamFactory>();
			_streamFactory.Create(Arg.Any<DatabricksOptions>(), Arg.Any<string>())
				.Returns(x => ErrorOrFactory.From(_stream));
			_log = Substitute.For<ILog>();
			_delays = new List<TimeSpan>();
			_publisher = new ChangePublisher(_optionsProvider, _streamFactory, () => DateTime.UtcNow, _log) {
				AutoStart = false,
				Delay = (delay, token) => {
					_delays.Add(delay);
					return true;
				}
			};
		}

		[TearDown]
		public void TearDown() {
			_publisher.Dispose();
		}

		private static ChangeRecord CreateRecord(string table = Table) {
			return new ChangeRecord {
				TargetTable = table,
				EntitySchemaName = "Contact",
				RecordId = Guid.NewGuid(),
				Operation = ChangeOperation.Insert,
				ChangedOnUtc = DateTime.UtcNow
			};
		}

		[Test]
		public void DeliverPending_IngestsRecordJson() {
			ChangeRecord record = CreateRecord();
			_publisher.Enqueue(record, null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(1).Ingest(record.ToJson());
			_publisher.PendingCount.Should().Be(0);
		}

		[Test]
		public void DeliverPending_ReusesStreamPerTable() {
			_publisher.Enqueue(CreateRecord(), null);
			_publisher.Enqueue(CreateRecord(), null);
			_publisher.Enqueue(CreateRecord("workspace.default.other"), null);

			_publisher.DeliverPending(CancellationToken.None);

			_streamFactory.Received(1).Create(Arg.Any<DatabricksOptions>(), Table);
			_streamFactory.Received(1).Create(Arg.Any<DatabricksOptions>(), "workspace.default.other");
			_stream.Received(3).Ingest(Arg.Any<string>());
		}

		[Test]
		public void DeliverPending_NotConfigured_DropsWithoutOpeningStream() {
			_options.ClientSecret = string.Empty;
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_streamFactory.DidNotReceive().Create(Arg.Any<DatabricksOptions>(), Arg.Any<string>());
			_publisher.PendingCount.Should().Be(0);
			_log.Received(1).WarnFormat(Arg.Is<string>(s => s.Contains("not configured")), Arg.Any<object[]>());
		}

		[Test]
		public void DeliverPending_StreamFactoryError_DropsRecord() {
			_streamFactory.Create(Arg.Any<DatabricksOptions>(), Arg.Any<string>())
				.Returns(Error.Failure("Zerobus.NativeLoadFailed", "no native library"));
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.DidNotReceive().Ingest(Arg.Any<string>());
			_delays.Should().BeEmpty();
		}

		[Test]
		public void DeliverPending_RetryableFailure_RecreatesStreamAndRetries() {
			int calls = 0;
			_stream.Ingest(Arg.Any<string>()).Returns(x => {
				if (calls++ == 0) {
					throw new ZerobusException("unavailable", true);
				}
				return 1L;
			});
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(2).Ingest(Arg.Any<string>());
			_stream.Received(1).Dispose();
			_streamFactory.Received(2).Create(Arg.Any<DatabricksOptions>(), Table);
			_delays.Should().Equal(TimeSpan.FromSeconds(1));
		}

		[Test]
		public void DeliverPending_NonRetryableFailure_RetriesOnceThenDrops() {
			_stream.Ingest(Arg.Any<string>()).Returns(x => throw new ZerobusException("bad schema", false));
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(2).Ingest(Arg.Any<string>());
			_publisher.PendingCount.Should().Be(0);
		}

		[Test]
		public void DeliverPending_UnexpectedFailure_GivesUpAfterThreeAttempts() {
			_stream.Ingest(Arg.Any<string>()).Returns(x => throw new InvalidOperationException("boom"));
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(3).Ingest(Arg.Any<string>());
			_publisher.PendingCount.Should().Be(0);
		}

		[Test]
		public void DeliverPending_UnserializableRecord_DropsWithoutOpeningStream() {
			ChangeRecord record = CreateRecord();
			record.Columns = new[] { new ColumnChange("Bad", null, new UnserializableValue()) };
			_publisher.Enqueue(record, null);

			_publisher.DeliverPending(CancellationToken.None);

			_streamFactory.DidNotReceive().Create(Arg.Any<DatabricksOptions>(), Arg.Any<string>());
			_publisher.PendingCount.Should().Be(0);
		}

		private sealed class UnserializableValue {
			public override string ToString() => throw new InvalidOperationException("cannot format");
		}

		[Test]
		public void DeliverPending_RetryBackoff_IsExponentialAndCapped() {
			int calls = 0;
			_stream.Ingest(Arg.Any<string>()).Returns(x => {
				if (calls++ < 7) {
					throw new ZerobusException("unavailable", true);
				}
				return 1L;
			});
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_delays.Should().Equal(new[] { 1, 2, 4, 8, 16, 30, 30 }.Select(s => TimeSpan.FromSeconds(s)));
		}

		[Test]
		public void DeliverPending_CancelledDuringBackoff_Stops() {
			_stream.Ingest(Arg.Any<string>()).Returns(x => throw new ZerobusException("unavailable", true));
			_publisher.Delay = (delay, token) => false;
			_publisher.Enqueue(CreateRecord(), null);

			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(1).Ingest(Arg.Any<string>());
		}

		[Test]
		public void DeliverPending_SettingsChanged_ReopensStreams() {
			_publisher.Enqueue(CreateRecord(), null);
			_publisher.DeliverPending(CancellationToken.None);
			_options = new DatabricksOptions {
				ZerobusEndpoint = _options.ZerobusEndpoint,
				WorkspaceUrl = _options.WorkspaceUrl,
				ClientId = _options.ClientId,
				ClientSecret = "rotated"
			};

			_publisher.Enqueue(CreateRecord(), null);
			_publisher.DeliverPending(CancellationToken.None);

			_stream.Received(1).Close();
			_streamFactory.Received(2).Create(Arg.Any<DatabricksOptions>(), Table);
		}

		[Test]
		public void CloseStreams_ClosesAndDisposesOpenStreams() {
			_publisher.Enqueue(CreateRecord(), null);
			_publisher.DeliverPending(CancellationToken.None);

			_publisher.CloseStreams();

			_stream.Received(1).Close();
			_stream.Received(1).Dispose();
		}

		[Test]
		public void Enqueue_QueueFull_ReturnsFalse() {
			for (int i = 0; i < 10000; i++) {
				_publisher.Enqueue(CreateRecord(), null).Should().BeTrue();
			}

			_publisher.Enqueue(CreateRecord(), null).Should().BeFalse();
		}

		[Test]
		public void StartAndStop_DeliversQueuedRecordsOnWorker() {
			_publisher.Delay = (delay, token) => !token.WaitHandle.WaitOne(delay);
			var delivered = new ManualResetEventSlim();
			_stream.Ingest(Arg.Any<string>()).Returns(x => {
				delivered.Set();
				return 1L;
			});

			_publisher.Start();
			_publisher.Start();
			_publisher.Enqueue(CreateRecord(), null);

			delivered.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
			_publisher.Stop();
			_stream.Received(1).Close();
			_publisher.Stop();
		}
	}
}
