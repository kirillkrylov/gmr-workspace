namespace Terrasoft.Core.Process.Configuration
{

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.Configuration;
	using Terrasoft.Core.DB;
	using Terrasoft.Core.Entities;
	using Terrasoft.Core.Process;

	#region Class: UsrRunRevenueFeedStage

	[DesignModeProperty(Name = "RunId", Group = "", ValuesProvider = "ProcessSchemaParameterValueProvider", Editor="xtype=processschemaparametervalueedit;dataProvider=processschemaparametervalueprovider", ResourceManager = "5523ceff9aa54a81b5c4e3de800fbd0c", CaptionResourceItem = "Parameters.RunId.Caption", DescriptionResourceItem = "Parameters.RunId.Caption", UseSolutionStorage = true)]
	[DesignModeProperty(Name = "IsSuccess", Group = "", ValuesProvider = "ProcessSchemaParameterValueProvider", Editor="xtype=processschemaparametervalueedit;dataProvider=processschemaparametervalueprovider", ResourceManager = "5523ceff9aa54a81b5c4e3de800fbd0c", CaptionResourceItem = "Parameters.IsSuccess.Caption", DescriptionResourceItem = "Parameters.IsSuccess.Caption", UseSolutionStorage = true)]
	[DesignModeProperty(Name = "ResultMessage", Group = "", ValuesProvider = "ProcessSchemaParameterValueProvider", Editor="xtype=processschemaparametervalueedit;dataProvider=processschemaparametervalueprovider", ResourceManager = "5523ceff9aa54a81b5c4e3de800fbd0c", CaptionResourceItem = "Parameters.ResultMessage.Caption", DescriptionResourceItem = "Parameters.ResultMessage.Caption", UseSolutionStorage = true)]
	/// <exclude/>
	public class UsrRunRevenueFeedStage : ProcessUserTask
	{

		#region Constructors: Public

		public UsrRunRevenueFeedStage(UserConnection userConnection)
			: base(userConnection) {
			SchemaUId = new Guid("5523ceff-9aa5-4a81-b5c4-e3de800fbd0c");
		}

		#endregion

		#region Properties: Public

		public virtual Guid RunId {
			get;
			set;
		}

		public virtual bool IsSuccess {
			get;
			set;
		}

		public virtual string ResultMessage {
			get;
			set;
		}

		#endregion

		#region Methods: Protected

		protected override bool InternalExecute(ProcessExecutingContext context) {
			BatchFileProcessingApp.RevenueFeed.RevenueFeedStageOutcome outcome =
				BatchFileProcessingApp.RevenueFeed.RevenueFeedService.ExecuteStage(UserConnection, Name, RunId);
			IsSuccess = outcome.Succeeded;
			ResultMessage = outcome.Message;
			return true;
		}

		#endregion

		#region Methods: Public

		public override bool CompleteExecuting(params object[] parameters) {
			return base.CompleteExecuting(parameters);
		}

		public override void CancelExecuting(params object[] parameters) {
			base.CancelExecuting(parameters);
		}

		public override string GetExecutionData() {
			return string.Empty;
		}

		public override ProcessElementNotification GetNotificationData() {
			return base.GetNotificationData();
		}

		public override void WritePropertiesData(DataWriter writer) {
			writer.WriteStartObject(Name);
			base.WritePropertiesData(writer);
			if (Status == Core.Process.ProcessStatus.Inactive) {
				writer.WriteFinishObject();
				return;
			}
			if (!HasMapping("RunId")) {
				writer.WriteValue("RunId", RunId, Guid.Empty);
			}
			if (!HasMapping("IsSuccess")) {
				writer.WriteValue("IsSuccess", IsSuccess, false);
			}
			if (!HasMapping("ResultMessage")) {
				writer.WriteValue("ResultMessage", ResultMessage, null);
			}
			writer.WriteFinishObject();
		}

		#endregion

		#region Methods: Protected

		protected override void ApplyPropertiesDataValues(DataReader reader) {
			base.ApplyPropertiesDataValues(reader);
			switch (reader.CurrentName) {
				case "RunId":
					RunId = reader.GetGuidValue();
				break;
				case "IsSuccess":
					IsSuccess = reader.GetBoolValue();
				break;
				case "ResultMessage":
					ResultMessage = reader.GetStringValue();
				break;
			}
		}

		#endregion

	}

	#endregion

}

