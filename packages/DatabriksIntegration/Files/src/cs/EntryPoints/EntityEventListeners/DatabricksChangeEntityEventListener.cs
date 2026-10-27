using System;
using Common.Logging;
using DatabriksIntegrationApp.Sync;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Entities.Events;

namespace DatabriksIntegrationApp.EntryPoints.EntityEventListeners {

	/// <summary>
	/// Global listener: receives inserts, updates and deletes of every object, and forwards those of the objects
	/// configured in the DatabricksSyncObject lookup to Databricks. It must never break the caller's save.
	/// </summary>
	[EntityEventListener(IsGlobal = true)]
	public class DatabricksChangeEntityEventListener : BaseEntityEventListener {

		public override void OnInserted(object sender, EntityAfterEventArgs e) {
			base.OnInserted(sender, e);
			Handle(sender, ChangeOperation.Insert);
		}

		public override void OnUpdated(object sender, EntityAfterEventArgs e) {
			base.OnUpdated(sender, e);
			Handle(sender, ChangeOperation.Update);
		}

		public override void OnDeleted(object sender, EntityAfterEventArgs e) {
			base.OnDeleted(sender, e);
			Handle(sender, ChangeOperation.Delete);
		}

		private static void Handle(object sender, ChangeOperation operation) {
			if (!(sender is Entity entity)) {
				return;
			}
			try {
				DatabriksIntegrationApp.Instance.GetRequiredService<IEntityChangeHandler>().Handle(entity, operation);
			} catch (Exception exception) {
				LogManager.GetLogger(Constants.LoggerName).Error(
					$"Failed to queue the Databricks change of {entity.Schema?.Name} {entity.PrimaryColumnValue}.",
					exception);
			}
		}
	}
}
