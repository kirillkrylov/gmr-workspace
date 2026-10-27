using System;
using BatchFileProcessingApp.RevenueFeed;
using Common.Logging;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Entities.Events;

namespace BatchFileProcessingApp.EntryPoints.EntityEventListeners {

	/// <summary>
	/// Starts processing when a file has finished uploading to a revenue feed run. Uploads arrive in chunks, so the
	/// listener waits for the save that completes the file. It must never break the caller's upload.
	/// </summary>
	[EntityEventListener(SchemaName = "SysFile")]
	public class RevenueFeedFileEventListener : BaseEntityEventListener {

		public override void OnSaved(object sender, EntityAfterEventArgs e) {
			base.OnSaved(sender, e);
			if (!(sender is Entity file)
					|| file.GetTypedColumnValue<string>("RecordSchemaName") != RevenueFeedStore.RunSchemaName) {
				return;
			}
			if (!RevenueFeedStore.IsUploadComplete(file.GetTypedColumnValue<long>("Size"),
					file.GetTypedColumnValue<long>("TotalSize"))) {
				return;
			}
			Guid runId = file.GetTypedColumnValue<Guid>("RecordId");
			try {
				RevenueFeedStarter.TryStart(file.UserConnection, runId);
			} catch (Exception exception) {
				LogManager.GetLogger(Constants.LoggerName).Error($"Could not start revenue feed run {runId}.", exception);
			}
		}
	}
}
