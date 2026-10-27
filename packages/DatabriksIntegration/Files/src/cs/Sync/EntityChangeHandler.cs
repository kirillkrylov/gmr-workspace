using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace DatabriksIntegrationApp.Sync {

	/// <summary>
	/// Turns an entity save or delete into a change record when its object is monitored.
	/// </summary>
	internal interface IEntityChangeHandler {
		void Handle(Entity entity, ChangeOperation operation);
	}

	internal sealed class EntityChangeHandler : IEntityChangeHandler {
		private readonly ISyncObjectRegistry _registry;
		private readonly IChangePublisher _publisher;
		private readonly Func<DateTime> _utcNow;

		public EntityChangeHandler(ISyncObjectRegistry registry, IChangePublisher publisher, Func<DateTime> utcNow) {
			_registry = registry;
			_publisher = publisher;
			_utcNow = utcNow;
		}

		public void Handle(Entity entity, ChangeOperation operation) {
			string schemaName = entity.Schema?.Name;
			if (string.Equals(schemaName, Constants.SyncObject.SchemaName, StringComparison.Ordinal)) {
				_registry.Invalidate();
				return;
			}
			UserConnection userConnection = entity.UserConnection;
			if (!_registry.TryGetTargetTable(userConnection, schemaName, out string targetTable)) {
				return;
			}
			IReadOnlyList<ColumnChange> columns = operation == ChangeOperation.Delete
				? (IReadOnlyList<ColumnChange>)Array.Empty<ColumnChange>()
				: entity.GetChangedColumnValues()
					.Select(value => new ColumnChange(value.Name, value.OldValue, value.Value))
					.ToList();
			if (operation == ChangeOperation.Update && columns.Count == 0) {
				return;
			}
			_publisher.Enqueue(new ChangeRecord {
				TargetTable = targetTable,
				EntitySchemaName = schemaName,
				RecordId = entity.PrimaryColumnValue,
				Operation = operation,
				ChangedOnUtc = _utcNow(),
				ChangedById = userConnection?.CurrentUser?.ContactId ?? Guid.Empty,
				Columns = columns
			}, userConnection);
		}
	}
}
