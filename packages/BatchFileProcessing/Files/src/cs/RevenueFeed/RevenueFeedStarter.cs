using System;
using System.Collections.Generic;
using Common.Logging;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Tasks;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// Starts the processing business process of a run in the background, once per upload.
	/// </summary>
	internal static class RevenueFeedStarter {

		/// <summary>
		/// Claims a run that is waiting for a file (or whose last file failed) and starts its processing in the
		/// background. Returns false when the run is not in a state that accepts a new file, or is already claimed.
		/// If the start fails, the claim is released by marking the run Failed, so a new upload can start it again.
		/// </summary>
		internal static bool TryStart(UserConnection userConnection, Guid runId) {
			var claim = (Update)new Update(userConnection, RevenueFeedStore.RunSchemaName)
				.Set("StatusId", Column.Parameter(RevenueFeedIds.RunStatus.Processing))
				.Where("Id").IsEqual(Column.Parameter(runId))
				.And("StatusId").In(Column.Parameters(RevenueFeedIds.RunStatus.WaitingForFile,
					RevenueFeedIds.RunStatus.Failed));
			if (claim.Execute() != 1) {
				return false;
			}
			try {
				Task.StartNewWithUserConnection<RevenueFeedProcessTask, Guid>(runId);
			} catch (Exception exception) {
				MarkStartFailed(userConnection, runId, exception);
				throw;
			}
			return true;
		}

		/// <summary>
		/// Records a run whose processing could not start as Failed with the reason, which also releases its claim.
		/// </summary>
		internal static void MarkStartFailed(UserConnection userConnection, Guid runId, Exception exception) {
			new Update(userConnection, RevenueFeedStore.RunSchemaName)
				.Set("StatusId", Column.Parameter(RevenueFeedIds.RunStatus.Failed))
				.Set("ErrorCodeId", Column.Parameter(RevenueFeedIds.ErrorCode.UnexpectedError))
				.Set("ErrorMessage", Column.Parameter(
					$"Processing could not start: {exception.GetType().Name}: {exception.Message}"))
				.Where("Id").IsEqual(Column.Parameter(runId))
				.Execute();
		}
	}

	/// <summary>
	/// Background task that runs the processing business process for one run.
	/// </summary>
	public sealed class RevenueFeedProcessTask : IBackgroundTask<Guid>, IUserConnectionRequired {

		private UserConnection _userConnection;

		public void Run(Guid runId) {
			try {
				_userConnection.ProcessEngine.ProcessExecutor.Execute(RevenueFeedService.ProcessName,
					new Dictionary<string, string> { { RevenueFeedService.RunIdParameter, runId.ToString() } });
			} catch (Exception exception) {
				BatchFileProcessingApp.Instance.GetRequiredService<ILog>()
					.Error($"Could not start {RevenueFeedService.ProcessName} for revenue feed run {runId}.", exception);
				RevenueFeedStarter.MarkStartFailed(_userConnection, runId, exception);
			}
		}

		public void SetUserConnection(UserConnection userConnection) {
			_userConnection = userConnection;
		}
	}
}
