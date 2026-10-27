using System;
using System.Collections.Generic;
using System.Linq;
using DatabriksIntegrationApp.Sync;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace DatabriksIntegration.Tests.Sync {

	[TestFixture(Category = "UnitTests")]
	public class EntityChangeHandlerTests : BaseComposableAppTestFixture {
		private const string Table = "workspace.default.creatio_changes";
		private static readonly DateTime Now = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);
		private ISyncObjectRegistry _registry;
		private IChangePublisher _publisher;
		private EntityChangeHandler _handler;
		private ChangeRecord _published;

		protected override void SetUp() {
			base.SetUp();
			MockEntitySchemaWithColumns("Contact", new Dictionary<string, DataValueType> {
				{ "Name", DataValueType.MediumText }
			});
			MockEntitySchemaWithColumns("DatabricksSyncObject", new Dictionary<string, DataValueType> {
				{ "Name", DataValueType.MediumText }
			});
			_registry = Substitute.For<ISyncObjectRegistry>();
			_registry.TryGetTargetTable(Arg.Any<UserConnection>(), "Contact", out Arg.Any<string>())
				.Returns(x => {
					x[2] = Table;
					return true;
				});
			_publisher = Substitute.For<IChangePublisher>();
			_publisher.Enqueue(Arg.Do<ChangeRecord>(record => _published = record), Arg.Any<UserConnection>())
				.Returns(true);
			_handler = new EntityChangeHandler(_registry, _publisher, () => Now);
			_published = null;
		}

		private Entity CreateContact(Guid id, string name) {
			return CreateEntity("Contact", new Dictionary<string, object> {
				{ "Id", id },
				{ "Name", name }
			});
		}

		[Test]
		public void Handle_MonitoredInsert_PublishesChangedColumns() {
			Guid id = Guid.NewGuid();
			Entity contact = CreateContact(id, "John");

			_handler.Handle(contact, ChangeOperation.Insert);

			_published.Should().NotBeNull();
			_published.TargetTable.Should().Be(Table);
			_published.EntitySchemaName.Should().Be("Contact");
			_published.RecordId.Should().Be(id);
			_published.Operation.Should().Be(ChangeOperation.Insert);
			_published.ChangedOnUtc.Should().Be(Now);
			_published.Columns.Should().Contain(column => column.Name == "Name" && (string)column.NewValue == "John");
		}

		[Test]
		public void Handle_Delete_PublishesWithoutColumns() {
			Guid id = Guid.NewGuid();
			Entity contact = CreateContact(id, "John");

			_handler.Handle(contact, ChangeOperation.Delete);

			_published.Operation.Should().Be(ChangeOperation.Delete);
			_published.RecordId.Should().Be(id);
			_published.Columns.Should().BeEmpty();
		}

		[Test]
		public void Handle_UpdateWithoutChangedColumns_DoesNotPublish() {
			Entity contact = CreateEmptyEntity("Contact");

			_handler.Handle(contact, ChangeOperation.Update);

			_publisher.DidNotReceive().Enqueue(Arg.Any<ChangeRecord>(), Arg.Any<UserConnection>());
		}

		[Test]
		public void Handle_UnmonitoredObject_DoesNotPublish() {
			_registry.TryGetTargetTable(Arg.Any<UserConnection>(), "Contact", out Arg.Any<string>()).Returns(false);

			_handler.Handle(CreateContact(Guid.NewGuid(), "John"), ChangeOperation.Insert);

			_publisher.DidNotReceive().Enqueue(Arg.Any<ChangeRecord>(), Arg.Any<UserConnection>());
		}

		[Test]
		public void Handle_SyncObjectChange_InvalidatesRegistry() {
			Entity syncObject = CreateEntity("DatabricksSyncObject", new Dictionary<string, object> {
				{ "Name", "Contacts" }
			});

			_handler.Handle(syncObject, ChangeOperation.Update);

			_registry.Received(1).Invalidate();
			_publisher.DidNotReceive().Enqueue(Arg.Any<ChangeRecord>(), Arg.Any<UserConnection>());
		}
	}
}
