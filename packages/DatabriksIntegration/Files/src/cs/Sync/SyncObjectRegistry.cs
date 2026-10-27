using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.Logging;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace DatabriksIntegrationApp.Sync {

	/// <summary>
	/// Reads the active rows of the DatabricksSyncObject lookup.
	/// </summary>
	internal interface ISyncObjectSource {
		/// <summary>Maps monitored entity schema name to its Databricks target table.</summary>
		IReadOnlyDictionary<string, string> Load(UserConnection userConnection);
	}

	// ESQ glue; the registry that consumes it is unit tested.
	[ExcludeFromCodeCoverage]
	internal sealed class EsqSyncObjectSource : ISyncObjectSource {

		public IReadOnlyDictionary<string, string> Load(UserConnection userConnection) {
			UserConnection systemConnection = userConnection.AppConnection?.SystemUserConnection ?? userConnection;
			var esq = new EntitySchemaQuery(systemConnection.EntitySchemaManager, Constants.SyncObject.SchemaName);
			string schemaNameColumn = esq.AddColumn(Constants.SyncObject.EntitySchemaNameColumn).Name;
			string targetTableColumn = esq.AddColumn(Constants.SyncObject.TargetTableColumn).Name;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal,
				Constants.SyncObject.IsActiveColumn, true));
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (Entity row in esq.GetEntityCollection(systemConnection)) {
				string schemaName = row.GetTypedColumnValue<string>(schemaNameColumn)?.Trim();
				string targetTable = row.GetTypedColumnValue<string>(targetTableColumn)?.Trim();
				if (!string.IsNullOrEmpty(schemaName) && !string.IsNullOrEmpty(targetTable)) {
					result[schemaName] = targetTable;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Answers "is this object monitored, and where do its changes go" from an in-memory snapshot.
	/// The check runs on every entity save in the system, so it must be a dictionary hit.
	/// </summary>
	internal interface ISyncObjectRegistry {
		bool TryGetTargetTable(UserConnection userConnection, string entitySchemaName, out string targetTable);

		/// <summary>Drops the snapshot; the next check reloads it.</summary>
		void Invalidate();
	}

	internal sealed class SyncObjectRegistry : ISyncObjectRegistry {
		private static readonly IReadOnlyDictionary<string, string> Empty =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private readonly object _syncRoot = new object();
		private readonly ISyncObjectSource _source;
		private readonly Func<DateTime> _utcNow;
		private readonly ILog _log;
		private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(Constants.Publisher.RegistryCacheSeconds);
		private volatile Snapshot _snapshot;

		public SyncObjectRegistry(ISyncObjectSource source, Func<DateTime> utcNow, ILog log) {
			_source = source;
			_utcNow = utcNow;
			_log = log;
		}

		public bool TryGetTargetTable(UserConnection userConnection, string entitySchemaName,
				out string targetTable) {
			targetTable = null;
			if (string.IsNullOrEmpty(entitySchemaName)) {
				return false;
			}
			return GetObjects(userConnection).TryGetValue(entitySchemaName, out targetTable);
		}

		public void Invalidate() {
			_snapshot = null;
		}

		private IReadOnlyDictionary<string, string> GetObjects(UserConnection userConnection) {
			Snapshot snapshot = _snapshot;
			if (snapshot != null && snapshot.ExpiresOnUtc > _utcNow()) {
				return snapshot.Objects;
			}
			lock (_syncRoot) {
				snapshot = _snapshot;
				if (snapshot != null && snapshot.ExpiresOnUtc > _utcNow()) {
					return snapshot.Objects;
				}
				IReadOnlyDictionary<string, string> objects;
				try {
					objects = _source.Load(userConnection);
				} catch (Exception exception) {
					// Never break the caller's save; retry after the cache period.
					_log.Error("Failed to load Databricks sync objects; changes are not published until the next reload.",
						exception);
					objects = Empty;
				}
				_snapshot = new Snapshot(objects, _utcNow() + _cacheDuration);
				return objects;
			}
		}

		private sealed class Snapshot {
			internal Snapshot(IReadOnlyDictionary<string, string> objects, DateTime expiresOnUtc) {
				Objects = objects;
				ExpiresOnUtc = expiresOnUtc;
			}

			internal IReadOnlyDictionary<string, string> Objects { get; }
			internal DateTime ExpiresOnUtc { get; }
		}
	}
}
