using System;
using BatchFileProcessingApp.RevenueFeed;
using Common.Logging;
using Terrasoft.Core;
using Terrasoft.Core.Scheduler;
using Terrasoft.Web.Common;

namespace BatchFileProcessingApp.EntryPoints.ApplicationListeners {

	/// <summary>
	/// Schedules the missed-slot check every 15 minutes. Re-scheduling an existing job only updates it.
	/// </summary>
	public sealed class RevenueFeedAppEventListener : AppEventListenerBase {

		private const int CheckPeriodMinutes = 15;

		public override void OnAppStart(AppEventContext context) {
			try {
				UserConnection userConnection = context.Application["AppConnection"] is AppConnection appConnection
					? appConnection.SystemUserConnection
					: null;
				if (userConnection == null) {
					return;
				}
				AppScheduler.ScheduleMinutelyJob<RevenueFeedMissedRunJob>(RevenueFeedMissedRunJob.JobGroup,
					userConnection.Workspace.Name, userConnection.CurrentUser.Name, CheckPeriodMinutes, null, true);
			} catch (Exception exception) {
				LogManager.GetLogger(Constants.LoggerName).Error("Could not schedule the revenue feed missed-slot check.",
					exception);
			}
		}
	}
}
