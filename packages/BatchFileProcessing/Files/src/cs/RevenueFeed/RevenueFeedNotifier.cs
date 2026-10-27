using System;
using System.Collections.Generic;
using System.Linq;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// Sends a bell notification about a failed, partly failed or missed run to every member of the System administrators role.
	/// </summary>
	internal sealed class RevenueFeedNotifier : IRevenueFeedNotifier {

		private static readonly Guid SystemAdministratorsRoleId = new Guid("83a43ebc-f36b-1410-298d-001e8c82bcad");
		private static readonly Guid NotificationTypeId = new Guid("685e7149-c015-4a4d-b4a6-2e5625a6314c");

		private readonly UserConnection _userConnection;

		public RevenueFeedNotifier(UserConnection userConnection) {
			_userConnection = userConnection;
		}

		public void NotifyProblem(Guid runId, string runName, Guid statusId, string message) {
			string title = statusId == RevenueFeedIds.RunStatus.Failed ? $"Revenue feed run {runName} failed"
				: statusId == RevenueFeedIds.RunStatus.Missed ? $"Revenue feed file missed ({runName})"
				: $"Revenue feed run {runName} completed with errors";
			EntitySchema remindingSchema = _userConnection.EntitySchemaManager.GetInstanceByName("Reminding");
			DateTime now = _userConnection.CurrentUser.GetCurrentDateTime();
			foreach (Guid contactId in GetAdministratorContacts()) {
				Entity reminding = remindingSchema.CreateEntity(_userConnection);
				reminding.SetDefColumnValues();
				reminding.SetColumnValue("AuthorId", _userConnection.CurrentUser.ContactId);
				reminding.SetColumnValue("ContactId", contactId);
				reminding.SetColumnValue("RemindTime", now);
				reminding.SetColumnValue("NotificationTypeId", NotificationTypeId);
				reminding.SetColumnValue("SubjectCaption", title);
				reminding.SetColumnValue("PopupTitle", title);
				reminding.SetColumnValue("Description", Truncate(message, 250));
				reminding.SetColumnValue("SubjectId", runId);
				reminding.SetColumnValue("IsRead", false);
				reminding.Save(false);
			}
		}

		private IEnumerable<Guid> GetAdministratorContacts() {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "SysUserInRole");
			EntitySchemaQueryColumn contact = esq.AddColumn("SysUser.Contact.Id");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "SysRole", SystemAdministratorsRoleId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "SysUser.Active", true));
			return esq.GetEntityCollection(_userConnection)
				.Select(e => e.GetTypedColumnValue<Guid>(contact.Name))
				.Where(id => id != Guid.Empty)
				.Distinct()
				.ToList();
		}

		private static string Truncate(string value, int length) {
			return string.IsNullOrEmpty(value) || value.Length <= length ? value : value.Substring(0, length - 1) + "…";
		}
	}
}
