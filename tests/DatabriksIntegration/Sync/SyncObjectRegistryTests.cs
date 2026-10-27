using System;
using System.Collections.Generic;
using Common.Logging;
using DatabriksIntegrationApp.Sync;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Terrasoft.Core;

namespace DatabriksIntegration.Tests.Sync {

	[TestFixture(Category = "UnitTests")]
	public class SyncObjectRegistryTests {
		private ISyncObjectSource _source;
		private ILog _log;
		private DateTime _now;
		private SyncObjectRegistry _registry;

		[SetUp]
		public void SetUp() {
			_source = Substitute.For<ISyncObjectSource>();
			_source.Load(Arg.Any<UserConnection>()).Returns(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
				{ "Contact", "workspace.default.creatio_changes" }
			});
			_log = Substitute.For<ILog>();
			_now = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
			_registry = new SyncObjectRegistry(_source, () => _now, _log);
		}

		[Test]
		public void TryGetTargetTable_MonitoredObject_ReturnsTable() {
			_registry.TryGetTargetTable(null, "Contact", out string table).Should().BeTrue();
			table.Should().Be("workspace.default.creatio_changes");
		}

		[Test]
		public void TryGetTargetTable_IgnoresCase() {
			_registry.TryGetTargetTable(null, "contact", out _).Should().BeTrue();
		}

		[Test]
		public void TryGetTargetTable_UnknownObject_ReturnsFalse() {
			_registry.TryGetTargetTable(null, "Account", out string table).Should().BeFalse();
			table.Should().BeNull();
		}

		[Test]
		public void TryGetTargetTable_EmptySchemaName_DoesNotLoad() {
			_registry.TryGetTargetTable(null, string.Empty, out _).Should().BeFalse();
			_source.DidNotReceive().Load(Arg.Any<UserConnection>());
		}

		[Test]
		public void TryGetTargetTable_WithinCachePeriod_LoadsOnce() {
			_registry.TryGetTargetTable(null, "Contact", out _);
			_now = _now.AddSeconds(30);
			_registry.TryGetTargetTable(null, "Account", out _);

			_source.Received(1).Load(Arg.Any<UserConnection>());
		}

		[Test]
		public void TryGetTargetTable_AfterCachePeriod_Reloads() {
			_registry.TryGetTargetTable(null, "Contact", out _);
			_now = _now.AddMinutes(2);
			_registry.TryGetTargetTable(null, "Contact", out _);

			_source.Received(2).Load(Arg.Any<UserConnection>());
		}

		[Test]
		public void Invalidate_ForcesReload() {
			_registry.TryGetTargetTable(null, "Contact", out _);
			_registry.Invalidate();
			_registry.TryGetTargetTable(null, "Contact", out _);

			_source.Received(2).Load(Arg.Any<UserConnection>());
		}

		[Test]
		public void TryGetTargetTable_SourceFails_ReturnsFalseAndLogs() {
			_source.Load(Arg.Any<UserConnection>()).Returns(x => throw new InvalidOperationException("db down"));

			_registry.TryGetTargetTable(null, "Contact", out _).Should().BeFalse();
			_log.Received(1).Error(Arg.Any<object>(), Arg.Any<Exception>());
		}
	}
}
