using DatabriksIntegrationApp.Sync;
using Terrasoft.Web.Common;

namespace DatabriksIntegrationApp.EntryPoints.ApplicationListeners {

	/// <summary>
	/// Starts the Databricks publisher worker with the host and stops it, bounded, on graceful shutdown.
	/// </summary>
	public sealed class DatabricksAppEventListener : AppEventListenerBase {

		public override void OnAppStart(AppEventContext context) {
			DatabriksIntegrationApp.Instance.GetRequiredService<IChangePublisher>().Start();
		}

		public override void OnAppEnd(AppEventContext context) {
			DatabriksIntegrationApp.Instance.GetRequiredService<IChangePublisher>().Stop();
		}
	}
}
