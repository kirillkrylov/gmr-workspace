using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrasoft.Common;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;
using Terrasoft.File;
using Terrasoft.File.Abstractions;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// <see cref="IRevenueFeedStore"/> over the Creatio database. Records are saved through entities so that the change
	/// log and record rights apply as for any other change.
	/// </summary>
	internal sealed class RevenueFeedStore : IRevenueFeedStore {

		internal const string RunSchemaName = "RevenueFeedRun";
		private const string StepSchemaName = "RevenueFeedRunStep";
		private const string LineSchemaName = "RevenueFeedLine";

		private readonly UserConnection _userConnection;

		public RevenueFeedStore(UserConnection userConnection) {
			_userConnection = userConnection;
		}

		public RevenueFeedRunInfo FindRun(Guid runId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, RunSchemaName);
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			EntitySchemaQueryColumn name = esq.AddColumn("Name");
			EntitySchemaQueryColumn status = esq.AddColumn("Status");
			EntitySchemaQueryColumn scheduledOn = esq.AddColumn("ScheduledOn");
			EntitySchemaQueryColumn startedOn = esq.AddColumn("StartedOn");
			EntitySchemaQueryColumn attempt = esq.AddColumn("Attempt");
			EntitySchemaQueryColumn errorCode = esq.AddColumn("ErrorCode");
			EntitySchemaQueryColumn fileId = esq.AddColumn("FileId");
			Entity run = esq.GetEntity(_userConnection, runId);
			if (run == null) {
				return null;
			}
			return new RevenueFeedRunInfo {
				Id = runId,
				Name = run.GetTypedColumnValue<string>(name.Name),
				StatusId = run.GetTypedColumnValue<Guid>(status.ValueQueryAlias),
				ScheduledOn = GetNullableDate(run, scheduledOn.Name),
				StartedOn = GetNullableDate(run, startedOn.Name),
				Attempt = run.GetTypedColumnValue<int>(attempt.Name),
				ErrorCodeId = GetNullableGuid(run, errorCode.ValueQueryAlias),
				FileId = GetNullableGuid(run, fileId.Name)
			};
		}

		public RevenueFeedFile FindLatestFile(Guid runId) {
			EntitySchemaQuery esq = CreateFileQuery();
			esq.RowCount = 20;
			esq.Columns.GetByName("CreatedOn").OrderByDesc();
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "RecordSchemaName", RunSchemaName));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "RecordId", runId));
			Entity fileRecord = esq.GetEntityCollection(_userConnection).FirstOrDefault(IsUploadComplete);
			return fileRecord == null ? null : ReadFile(fileRecord);
		}

		public RevenueFeedFile FindFile(Guid fileId) {
			Entity fileRecord = CreateFileQuery().GetEntity(_userConnection, fileId);
			return fileRecord == null ? null : ReadFile(fileRecord);
		}

		/// <summary>Uploads arrive in chunks: Size grows until it reaches TotalSize.</summary>
		internal static bool IsUploadComplete(long size, long totalSize) {
			return totalSize <= 0 || size >= totalSize;
		}

		private static bool IsUploadComplete(Entity fileRecord) {
			return IsUploadComplete(fileRecord.GetTypedColumnValue<long>("Size"),
				fileRecord.GetTypedColumnValue<long>("TotalSize"));
		}

		private EntitySchemaQuery CreateFileQuery() {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "SysFile");
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("Name");
			esq.AddColumn("Size");
			esq.AddColumn("TotalSize");
			esq.AddColumn("CreatedOn");
			return esq;
		}

		private RevenueFeedFile ReadFile(Entity fileRecord) {
			Guid fileId = fileRecord.PrimaryColumnValue;
			IFile file = _userConnection.GetFileFactory().Get(new EntityFileLocator("SysFile", fileId));
			using (Stream stream = file.Read())
			using (var buffer = new MemoryStream()) {
				stream.CopyTo(buffer);
				return new RevenueFeedFile {
					Id = fileId, Name = fileRecord.GetTypedColumnValue<string>("Name"), Content = buffer.ToArray()
				};
			}
		}

		public string FindRunWithSameFile(Guid runId, string fileHash) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, RunSchemaName);
			esq.RowCount = 1;
			EntitySchemaQueryColumn name = esq.AddColumn("Name");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "FileHash", fileHash));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Id", runId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Status",
				RevenueFeedIds.RunStatus.Succeeded, RevenueFeedIds.RunStatus.CompletedWithErrors));
			Entity run = esq.GetEntityCollection(_userConnection).FirstOrDefault();
			return run?.GetTypedColumnValue<string>(name.Name);
		}

		public void UpdateRun(Guid runId, RevenueFeedRunUpdate update) {
			Entity run = Fetch(RunSchemaName, runId);
			if (run == null) {
				return;
			}
			SetIfHasValue(run, "StatusId", update.StatusId);
			SetIfHasValue(run, "StageId", update.StageId);
			SetIfHasValue(run, "ScheduledOn", update.ScheduledOn);
			SetIfHasValue(run, "ReceivedOn", update.ReceivedOn);
			SetIfHasValue(run, "StartedOn", update.StartedOn);
			SetIfHasValue(run, "CompletedOn", update.CompletedOn);
			SetIfHasValue(run, "Duration", update.Duration);
			SetIfHasValue(run, "LineCount", update.LineCount);
			SetIfHasValue(run, "AppliedCount", update.AppliedCount);
			SetIfHasValue(run, "RejectedCount", update.RejectedCount);
			SetIfHasValue(run, "SkippedCount", update.SkippedCount);
			SetIfHasValue(run, "Attempt", update.Attempt);
			SetIfHasValue(run, "FileId", update.FileId);
			if (update.FileName != null) {
				run.SetColumnValue("FileName", update.FileName);
			}
			if (update.FileHash != null) {
				run.SetColumnValue("FileHash", update.FileHash);
			}
			if (update.ErrorCodeId.HasValue) {
				run.SetColumnValue("ErrorCodeId", update.ErrorCodeId == Guid.Empty ? (object)null : update.ErrorCodeId.Value);
			}
			if (update.ErrorMessage != null) {
				run.SetColumnValue("ErrorMessage", update.ErrorMessage);
			}
			Save(run);
		}

		public void AddStep(RevenueFeedStepRecord step) {
			Entity entity = New(StepSchemaName);
			entity.SetColumnValue("Name", step.Name);
			entity.SetColumnValue("RunId", step.RunId);
			entity.SetColumnValue("StageId", step.StageId);
			entity.SetColumnValue("ResultId", step.ResultId);
			entity.SetColumnValue("StartedOn", step.StartedOn);
			entity.SetColumnValue("CompletedOn", step.CompletedOn);
			entity.SetColumnValue("Duration", (int)Math.Round((step.CompletedOn - step.StartedOn).TotalMilliseconds));
			entity.SetColumnValue("Message", step.Message ?? string.Empty);
			entity.SetColumnValue("Attempt", step.Attempt);
			Save(entity);
		}

		public IList<RevenueFeedLineRecord> GetLines(Guid runId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, LineSchemaName);
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			string[] columns = {
				"Name", "LineNumber", "RawText", "BillingReference", "AccountCode", "OpportunityCode", "StartDate",
				"EndDate", "Amount", "ErrorMessage", "PreviousAmount", "NewAmount", "AppliedOn"
			};
			foreach (string column in columns) {
				esq.AddColumn(column);
			}
			EntitySchemaQueryColumn status = esq.AddColumn("Status");
			EntitySchemaQueryColumn errorCode = esq.AddColumn("ErrorCode");
			EntitySchemaQueryColumn account = esq.AddColumn("Account");
			EntitySchemaQueryColumn opportunity = esq.AddColumn("Opportunity");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Run", runId));
			return esq.GetEntityCollection(_userConnection).Select(e => new RevenueFeedLineRecord {
				Id = e.PrimaryColumnValue,
				RunId = runId,
				Name = e.GetTypedColumnValue<string>("Name"),
				LineNumber = e.GetTypedColumnValue<int>("LineNumber"),
				RawText = e.GetTypedColumnValue<string>("RawText"),
				BillingReference = e.GetTypedColumnValue<string>("BillingReference"),
				AccountCode = e.GetTypedColumnValue<string>("AccountCode"),
				OpportunityCode = e.GetTypedColumnValue<string>("OpportunityCode"),
				StartDate = GetNullableDate(e, "StartDate"),
				EndDate = GetNullableDate(e, "EndDate"),
				Amount = e.GetTypedColumnValue<decimal>("Amount"),
				StatusId = e.GetTypedColumnValue<Guid>(status.ValueQueryAlias),
				ErrorCodeId = GetNullableGuid(e, errorCode.ValueQueryAlias),
				ErrorMessage = e.GetTypedColumnValue<string>("ErrorMessage"),
				AccountId = GetNullableGuid(e, account.ValueQueryAlias),
				OpportunityId = GetNullableGuid(e, opportunity.ValueQueryAlias),
				PreviousAmount = e.GetTypedColumnValue<decimal>("PreviousAmount"),
				NewAmount = e.GetTypedColumnValue<decimal>("NewAmount"),
				AppliedOn = GetNullableDate(e, "AppliedOn")
			}).ToList();
		}

		public void SaveLine(RevenueFeedLineRecord line) {
			Entity entity = Fetch(LineSchemaName, line.Id);
			if (entity == null) {
				entity = New(LineSchemaName);
				entity.SetColumnValue("Id", line.Id);
			}
			entity.SetColumnValue("Name", line.Name);
			entity.SetColumnValue("RunId", line.RunId);
			entity.SetColumnValue("LineNumber", line.LineNumber);
			entity.SetColumnValue("RawText", line.RawText ?? string.Empty);
			entity.SetColumnValue("BillingReference", line.BillingReference ?? string.Empty);
			entity.SetColumnValue("AccountCode", line.AccountCode ?? string.Empty);
			entity.SetColumnValue("OpportunityCode", line.OpportunityCode ?? string.Empty);
			entity.SetColumnValue("StartDate", line.StartDate);
			entity.SetColumnValue("EndDate", line.EndDate);
			entity.SetColumnValue("Amount", line.Amount.GetValueOrDefault());
			entity.SetColumnValue("StatusId", line.StatusId);
			entity.SetColumnValue("ErrorCodeId", line.ErrorCodeId);
			entity.SetColumnValue("ErrorMessage", line.ErrorMessage ?? string.Empty);
			entity.SetColumnValue("AccountId", line.AccountId);
			entity.SetColumnValue("OpportunityId", line.OpportunityId);
			entity.SetColumnValue("PreviousAmount", line.PreviousAmount.GetValueOrDefault());
			entity.SetColumnValue("NewAmount", line.NewAmount.GetValueOrDefault());
			entity.SetColumnValue("AppliedOn", line.AppliedOn);
			Save(entity);
		}

		public void DeleteLines(IEnumerable<Guid> lineIds) {
			foreach (Guid lineId in lineIds) {
				Fetch(LineSchemaName, lineId)?.Delete();
			}
		}

		public IDictionary<string, Guid> FindAccountsByCode(ICollection<string> codes) {
			var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
			if (codes.Count == 0) {
				return result;
			}
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "Account");
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("Code");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Code", codes.Cast<object>().ToArray()));
			foreach (Entity account in esq.GetEntityCollection(_userConnection)) {
				string code = account.GetTypedColumnValue<string>("Code");
				if (!result.ContainsKey(code)) {
					result.Add(code, account.PrimaryColumnValue);
				}
			}
			return result;
		}

		public IDictionary<string, RevenueFeedOpportunityInfo> FindOpportunitiesByCode(ICollection<string> codes) {
			var result = new Dictionary<string, RevenueFeedOpportunityInfo>(StringComparer.OrdinalIgnoreCase);
			if (codes.Count == 0) {
				return result;
			}
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "Opportunity");
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("RevenueSystemCode");
			EntitySchemaQueryColumn account = esq.AddColumn("Account");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "RevenueSystemCode",
				codes.Cast<object>().ToArray()));
			foreach (Entity opportunity in esq.GetEntityCollection(_userConnection)) {
				string code = opportunity.GetTypedColumnValue<string>("RevenueSystemCode");
				if (!result.ContainsKey(code)) {
					result.Add(code, new RevenueFeedOpportunityInfo {
						Id = opportunity.PrimaryColumnValue,
						Code = code,
						AccountId = GetNullableGuid(opportunity, account.ValueQueryAlias)
					});
				}
			}
			return result;
		}

		public string FindRunThatApplied(string billingReference, Guid excludingLineId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, LineSchemaName);
			esq.RowCount = 1;
			EntitySchemaQueryColumn runName = esq.AddColumn("Run.Name");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "BillingReference", billingReference));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Status",
				RevenueFeedIds.LineStatus.Applied));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Id", excludingLineId));
			Entity line = esq.GetEntityCollection(_userConnection).FirstOrDefault();
			return line?.GetTypedColumnValue<string>(runName.Name);
		}

		public decimal GetAppliedAmountForOpportunity(Guid opportunityId) {
			return SumAppliedAmount("Opportunity", opportunityId);
		}

		public decimal GetAppliedAmountForAccount(Guid accountId) {
			return SumAppliedAmount("Account", accountId);
		}

		public void SetOpportunityActual(Guid opportunityId, decimal actualAmount, Guid runId) {
			Entity opportunity = Fetch("Opportunity", opportunityId);
			if (opportunity == null) {
				return;
			}
			decimal projected = opportunity.GetTypedColumnValue<decimal>("Amount");
			decimal variance = actualAmount - projected;
			opportunity.SetColumnValue("ActualAmount", actualAmount);
			opportunity.SetColumnValue("AmountVariance", variance);
			opportunity.SetColumnValue("AmountVariancePercent",
				projected == 0m ? 0m : Math.Round(variance / projected * 100m, 2));
			opportunity.SetColumnValue("LastRevenueRunId", runId);
			Save(opportunity);
		}

		public void SetAccountActual(Guid accountId, decimal actualAmount, Guid runId) {
			Entity account = Fetch("Account", accountId);
			if (account == null) {
				return;
			}
			account.SetColumnValue("ActualAmount", actualAmount);
			account.SetColumnValue("LastRevenueRunId", runId);
			Save(account);
		}

		private decimal SumAppliedAmount(string ownerColumn, Guid ownerId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, LineSchemaName);
			EntitySchemaQueryColumn sum = esq.AddColumn(esq.CreateAggregationFunction(AggregationTypeStrict.Sum, "Amount"));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, ownerColumn, ownerId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Status",
				RevenueFeedIds.LineStatus.Applied));
			Entity result = esq.GetEntityCollection(_userConnection).FirstOrDefault();
			return result == null ? 0m : result.GetTypedColumnValue<decimal>(sum.Name);
		}

		/// <summary>
		/// Saves the entity; a save cancelled by a business rule or listener returns false and must not pass silently.
		/// </summary>
		private static void Save(Entity entity) {
			if (!entity.Save(false)) {
				throw new InvalidOperationException(
					$"Saving {entity.Schema.Name} {entity.PrimaryColumnValue} was cancelled by a business rule or listener.");
			}
		}

		private Entity Fetch(string schemaName, Guid id) {
			Entity entity = _userConnection.EntitySchemaManager.GetInstanceByName(schemaName).CreateEntity(_userConnection);
			return entity.FetchFromDB(id) ? entity : null;
		}

		private Entity New(string schemaName) {
			Entity entity = _userConnection.EntitySchemaManager.GetInstanceByName(schemaName).CreateEntity(_userConnection);
			entity.SetDefColumnValues();
			return entity;
		}

		private static void SetIfHasValue<T>(Entity entity, string column, T? value) where T : struct {
			if (value.HasValue) {
				entity.SetColumnValue(column, value.Value);
			}
		}

		private static DateTime? GetNullableDate(Entity entity, string column) {
			DateTime value = entity.GetTypedColumnValue<DateTime>(column);
			return value == default(DateTime) ? (DateTime?)null : value;
		}

		private static Guid? GetNullableGuid(Entity entity, string column) {
			Guid value = entity.GetTypedColumnValue<Guid>(column);
			return value == Guid.Empty ? (Guid?)null : value;
		}
	}
}
