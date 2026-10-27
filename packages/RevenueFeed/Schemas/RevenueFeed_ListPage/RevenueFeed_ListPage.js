define("RevenueFeed_ListPage", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
	return {
		viewConfigDiff: /**SCHEMA_VIEW_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"name": "MenuItem_ImportFromExcel",
				"values": {
					"clicked": {
						"request": "crt.ImportDataRequest",
						"params": {
							"entitySchemaName": "RevenueFeedRun"
						}
					}
				}
			},
			{
				"operation": "merge",
				"name": "FolderTree",
				"values": {
					"sourceSchemaName": "FolderTree",
					"rootSchemaName": "RevenueFeedRun"
				}
			},
			{
				"operation": "merge",
				"name": "DataTable",
				"values": {
					"columns": [
						{
							"id": "f252f581-0ccf-44ac-b7c9-c00df2ad9919",
							"code": "PDS_Name",
							"caption": "#ResourceString(PDS_Name)#",
							"dataValueType": 28,
							"width": 150
						},
						{
							"id": "6e98815c-4f29-4516-a725-8b694a642fa0",
							"code": "PDS_ScheduledOn",
							"caption": "#ResourceString(PDS_ScheduledOn)#",
							"dataValueType": 7,
							"width": 160
						},
						{
							"id": "2b9652eb-e940-4f48-ae97-5100126203d7",
							"code": "PDS_Status",
							"caption": "#ResourceString(PDS_Status)#",
							"dataValueType": 10,
							"width": 170,
							"referenceSchemaName": "RevenueFeedRunStatus"
						},
						{
							"id": "8d07d0da-0fb5-4262-a7d6-01f67214b30b",
							"code": "PDS_Stage",
							"caption": "#ResourceString(PDS_Stage)#",
							"dataValueType": 10,
							"width": 140,
							"referenceSchemaName": "RevenueFeedStage"
						},
						{
							"id": "6d57a830-3af4-4e10-a18c-714a07302434",
							"code": "PDS_FileName",
							"caption": "#ResourceString(PDS_FileName)#",
							"dataValueType": 28,
							"width": 200
						},
						{
							"id": "9586162d-308a-4992-91b8-698f92f4d134",
							"code": "PDS_LineCount",
							"caption": "#ResourceString(PDS_LineCount)#",
							"dataValueType": 4,
							"width": 90
						},
						{
							"id": "b53c9309-c11a-48ea-9e46-b2f21576b4a7",
							"code": "PDS_AppliedCount",
							"caption": "#ResourceString(PDS_AppliedCount)#",
							"dataValueType": 4,
							"width": 90
						},
						{
							"id": "292462c5-08ee-4db7-8ff5-4e69d7ac0ad3",
							"code": "PDS_RejectedCount",
							"caption": "#ResourceString(PDS_RejectedCount)#",
							"dataValueType": 4,
							"width": 90
						},
						{
							"id": "6541fbcf-99b2-44f5-9901-437aefcaefd5",
							"code": "PDS_ErrorCode",
							"caption": "#ResourceString(PDS_ErrorCode)#",
							"dataValueType": 10,
							"width": 200,
							"referenceSchemaName": "RevenueFeedErrorCode"
						},
						{
							"id": "34af268b-d39b-4c41-a77b-9c893421d52c",
							"code": "PDS_Duration",
							"caption": "#ResourceString(PDS_Duration)#",
							"dataValueType": 4,
							"width": 110
						},
						{
							"id": "651d90d8-54f2-45c9-bcf5-44fe9109cb1e",
							"code": "PDS_CompletedOn",
							"caption": "#ResourceString(PDS_CompletedOn)#",
							"dataValueType": 7,
							"width": 160
						}
					]
				}
			},
			{
				"operation": "merge",
				"name": "Dashboards",
				"values": {
					"_designOptions": {
						"entitySchemaName": "RevenueFeedRun",
						"dependencies": [
							{
								"attributePath": "Id",
								"relationPath": "PDS.Id"
							}
						],
						"filters": []
					}
				}
			}
		]/**SCHEMA_VIEW_CONFIG_DIFF*/,
		viewModelConfigDiff: /**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [
					"attributes",
					"Items",
					"viewModelConfig",
					"attributes"
				],
				"values": {
					"PDS_Name": {
						"modelConfig": {
							"path": "PDS.Name"
						}
					},
					"PDS_ScheduledOn": {
						"modelConfig": {
							"path": "PDS.ScheduledOn"
						}
					},
					"PDS_Status": {
						"modelConfig": {
							"path": "PDS.Status"
						}
					},
					"PDS_Stage": {
						"modelConfig": {
							"path": "PDS.Stage"
						}
					},
					"PDS_FileName": {
						"modelConfig": {
							"path": "PDS.FileName"
						}
					},
					"PDS_LineCount": {
						"modelConfig": {
							"path": "PDS.LineCount"
						}
					},
					"PDS_AppliedCount": {
						"modelConfig": {
							"path": "PDS.AppliedCount"
						}
					},
					"PDS_RejectedCount": {
						"modelConfig": {
							"path": "PDS.RejectedCount"
						}
					},
					"PDS_ErrorCode": {
						"modelConfig": {
							"path": "PDS.ErrorCode"
						}
					},
					"PDS_Duration": {
						"modelConfig": {
							"path": "PDS.Duration"
						}
					},
					"PDS_CompletedOn": {
						"modelConfig": {
							"path": "PDS.CompletedOn"
						}
					}
				}
			},
			{
				"operation": "merge",
				"path": [
					"attributes",
					"Items",
					"modelConfig",
					"sortingConfig"
				],
				"values": {
					"default": [
						{
							"columnName": "ScheduledOn",
							"direction": "desc"
						}
					]
				}
			}
		]/**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/,
		modelConfigDiff: /**SCHEMA_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [
					"dataSources",
					"PDS",
					"config"
				],
				"values": {
					"entitySchemaName": "RevenueFeedRun"
				}
			}
		]/**SCHEMA_MODEL_CONFIG_DIFF*/,
		handlers: /**SCHEMA_HANDLERS*/[]/**SCHEMA_HANDLERS*/,
		converters: /**SCHEMA_CONVERTERS*/{}/**SCHEMA_CONVERTERS*/,
		validators: /**SCHEMA_VALIDATORS*/{}/**SCHEMA_VALIDATORS*/
	};
});
