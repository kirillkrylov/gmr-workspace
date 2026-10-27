define("RevenueFeed_FormPage", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
	return {
		viewConfigDiff: /**SCHEMA_VIEW_CONFIG_DIFF*/[
			{
				"operation": "insert",
				"name": "Name",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.Name",
					"control": "$Name",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "SideAreaProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RunStatus",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_Status",
					"control": "$PDS_Status",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "SideAreaProfileContainer",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "RunStage",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_Stage",
					"control": "$PDS_Stage",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "SideAreaProfileContainer",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "merge",
				"name": "AttachmentList",
				"values": {
					"type": "crt.FileList",
					"masterRecordColumnValue": "$Id",
					"recordColumnName": "RecordId",
					"layoutConfig": {
						"colSpan": 2,
						"column": 1,
						"row": 1,
						"rowSpan": 6
					},
					"items": "$AttachmentList",
					"primaryColumnName": "AttachmentListDS_Id",
					"columns": [
						{
							"id": "e908c8fb-e4fc-4b07-b989-2a2486c5542f",
							"code": "AttachmentListDS_Name",
							"caption": "#ResourceString(AttachmentListDS_Name)#",
							"dataValueType": 28,
							"width": 200
						}
					],
					"viewType": "gallery",
					"tileSize": "small"
				},
				"parentName": "AttachmentsTabContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "merge",
				"name": "Feed",
				"values": {
					"type": "crt.Feed",
					"feedType": "Record",
					"primaryColumnValue": "$Id",
					"cardState": "$CardState",
					"dataSourceName": "PDS",
					"entitySchemaName": "RevenueFeedRun"
				},
				"parentName": "FeedTabContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "ReprocessButton",
				"values": {
					"type": "crt.Button",
					"caption": "$Resources.Strings.ReprocessButton_caption",
					"color": "primary",
					"icon": "reload-icon",
					"iconPosition": "left-icon",
					"size": "large",
					"visible": true,
					"clickMode": "default",
					"clicked": {
						"request": "crt.RunBusinessProcessRequest",
						"params": {
							"processName": "RevenueFeedRun_ProcessFile",
							"processRunType": "ForTheSelectedPage",
							"saveAtProcessStart": true,
							"showNotification": true,
							"recordIdProcessParameterName": "RunIdParameter"
						}
					}
				},
				"parentName": "ActionButtonsContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "merge",
				"name": "GeneralInfoTabContainer",
				"values": {
					"visible": false
				}
			},
			{
				"operation": "insert",
				"name": "RunDetailsExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RunDetailsExpansionPanel_title",
					"toggleType": "default",
					"togglePosition": "before",
					"expanded": true,
					"labelColor": "auto",
					"fullWidthHeader": false,
					"titleWidth": 20,
					"padding": {
						"top": "small",
						"bottom": "small",
						"left": "none",
						"right": "none"
					},
					"fitContent": true,
					"visible": true,
					"items": [],
					"tools": []
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "RunDetailsExpansionPanelGrid",
				"values": {
					"type": "crt.GridContainer",
					"rows": "minmax(max-content, 32px)",
					"columns": [
						"minmax(32px, 1fr)",
						"minmax(32px, 1fr)"
					],
					"gap": {
						"columnGap": "large",
						"rowGap": "none"
					},
					"items": []
				},
				"parentName": "RunDetailsExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RunScheduledOn",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_ScheduledOn",
					"control": "$PDS_ScheduledOn",
					"labelPosition": "auto"
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RunReceivedOn",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_ReceivedOn",
					"control": "$PDS_ReceivedOn",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "RunStartedOn",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_StartedOn",
					"control": "$PDS_StartedOn",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "RunCompletedOn",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_CompletedOn",
					"control": "$PDS_CompletedOn",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "RunDuration",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_Duration",
					"control": "$PDS_Duration",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "RunAttempt",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_Attempt",
					"control": "$PDS_Attempt",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "RunFileName",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 4,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_FileName",
					"control": "$PDS_FileName",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "RunFileHash",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 4,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_FileHash",
					"control": "$PDS_FileHash",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 7
			},
			{
				"operation": "insert",
				"name": "RunLineCount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 5,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_LineCount",
					"control": "$PDS_LineCount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 8
			},
			{
				"operation": "insert",
				"name": "RunAppliedCount",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 5,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_AppliedCount",
					"control": "$PDS_AppliedCount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 9
			},
			{
				"operation": "insert",
				"name": "RunRejectedCount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 6,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_RejectedCount",
					"control": "$PDS_RejectedCount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 10
			},
			{
				"operation": "insert",
				"name": "RunSkippedCount",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 6,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_SkippedCount",
					"control": "$PDS_SkippedCount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RunDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 11
			},
			{
				"operation": "insert",
				"name": "RunErrorExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RunErrorExpansionPanel_title",
					"toggleType": "default",
					"togglePosition": "before",
					"expanded": true,
					"labelColor": "auto",
					"fullWidthHeader": false,
					"titleWidth": 20,
					"padding": {
						"top": "small",
						"bottom": "small",
						"left": "none",
						"right": "none"
					},
					"fitContent": true,
					"visible": true,
					"items": [],
					"tools": []
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "RunErrorExpansionPanelGrid",
				"values": {
					"type": "crt.GridContainer",
					"rows": "minmax(max-content, 32px)",
					"columns": [
						"minmax(32px, 1fr)",
						"minmax(32px, 1fr)"
					],
					"gap": {
						"columnGap": "large",
						"rowGap": "none"
					},
					"items": []
				},
				"parentName": "RunErrorExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RunErrorCode",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_ErrorCode",
					"control": "$PDS_ErrorCode",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "RunErrorExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RunErrorMessage",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 2,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_ErrorMessage",
					"control": "$PDS_ErrorMessage",
					"labelPosition": "auto",
					"multiline": true,
					"readonly": true
				},
				"parentName": "RunErrorExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "RunStepsExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RunStepsExpansionPanel_title",
					"toggleType": "default",
					"togglePosition": "before",
					"expanded": true,
					"labelColor": "auto",
					"fullWidthHeader": true,
					"titleWidth": 20,
					"padding": {
						"top": "small",
						"bottom": "small",
						"left": "none",
						"right": "none"
					},
					"fitContent": true,
					"visible": true,
					"items": [],
					"tools": []
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "RunStepsListContainer",
				"parentName": "RunStepsExpansionPanel",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.GridContainer",
					"columns": [
						"minmax(32px, 1fr)",
						"minmax(32px, 1fr)"
					],
					"rows": "minmax(max-content, 32px)",
					"gap": {
						"columnGap": "large",
						"rowGap": 0
					},
					"styles": {
						"overflow-x": "hidden"
					},
					"items": []
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsList",
				"parentName": "RunStepsListContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.DataGrid",
					"layoutConfig": {
						"colSpan": 2,
						"column": 1,
						"row": 1,
						"rowSpan": 6
					},
					"items": "$RunStepsList",
					"activeRow": "$RunStepsList_ActiveRow",
					"primaryColumnName": "RunStepsListDS_Id",
					"fitContent": true,
					"visible": true,
					"columns": [
						{
							"id": "65f869be-3078-422b-b566-f39c94759611",
							"code": "RunStepsListDS_Name",
							"caption": "#ResourceString(RunStepsListDS_Name)#",
							"dataValueType": 28,
							"width": 200
						},
						{
							"id": "0065da36-4a0e-4123-9d36-c6e37aa0568c",
							"code": "RunStepsListDS_Result",
							"caption": "#ResourceString(RunStepsListDS_Result)#",
							"dataValueType": 10,
							"width": 170,
							"referenceSchemaName": "RevenueFeedStepResult"
						},
						{
							"id": "0d17932f-5e3c-4f45-845f-415aa44d6950",
							"code": "RunStepsListDS_StartedOn",
							"caption": "#ResourceString(RunStepsListDS_StartedOn)#",
							"dataValueType": 7,
							"width": 170
						},
						{
							"id": "af9870f5-b0dc-402e-a070-f7e2c62d63c1",
							"code": "RunStepsListDS_Duration",
							"caption": "#ResourceString(RunStepsListDS_Duration)#",
							"dataValueType": 4,
							"width": 120
						},
						{
							"id": "b3ae92d9-45e0-476e-b7fd-94b528f0c692",
							"code": "RunStepsListDS_Message",
							"caption": "#ResourceString(RunStepsListDS_Message)#",
							"dataValueType": 29,
							"width": 400
						},
						{
							"id": "a553c5ca-22e6-473c-8ce4-d9cb57fe78fd",
							"code": "RunStepsListDS_Attempt",
							"caption": "#ResourceString(RunStepsListDS_Attempt)#",
							"dataValueType": 4,
							"width": 90
						}
					],
					"features": {
						"rows": {
							"selection": {
								"enable": true,
								"multiple": true
							}
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsToolsContainer",
				"parentName": "RunStepsExpansionPanel",
				"propertyName": "tools",
				"index": 0,
				"values": {
					"type": "crt.GridContainer",
					"rows": "minmax(max-content, 24px)",
					"columns": [
						"minmax(32px, 1fr)"
					],
					"gap": {
						"columnGap": "large",
						"rowGap": "none"
					},
					"color": "transparent",
					"items": []
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsToolsFlexContainer",
				"parentName": "RunStepsToolsContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.FlexContainer",
					"direction": "row",
					"alignItems": "center",
					"gap": "none",
					"items": [],
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsRefreshButton",
				"parentName": "RunStepsToolsFlexContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.Button",
					"icon": "reload-icon",
					"iconPosition": "only-icon",
					"color": "default",
					"clicked": {
						"request": "crt.LoadDataRequest",
						"params": {
							"config": {
								"loadType": "reload"
							},
							"dataSourceName": "RunStepsListDS"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsSettingsButton",
				"parentName": "RunStepsToolsFlexContainer",
				"propertyName": "items",
				"index": 1,
				"values": {
					"type": "crt.Button",
					"icon": "actions-button-icon",
					"iconPosition": "only-icon",
					"color": "default",
					"clickMode": "menu",
					"menuItems": []
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsExportMenuItem",
				"parentName": "RunStepsSettingsButton",
				"propertyName": "menuItems",
				"index": 0,
				"values": {
					"type": "crt.MenuItem",
					"icon": "export-button-icon",
					"caption": "#ResourceString(RunStepsExportMenuItem_caption)#",
					"clicked": {
						"request": "crt.ExportDataGridToExcelRequest",
						"params": {
							"viewName": "RunStepsList"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunStepsSearchFilter",
				"parentName": "RunStepsToolsFlexContainer",
				"propertyName": "items",
				"index": 2,
				"values": {
					"type": "crt.SearchFilter",
					"iconOnly": true,
					"placeholder": "#ResourceString(RunStepsSearchFilter_placeholder)#",
					"_filterOptions": {
						"expose": [
							{
								"attribute": "RunStepsSearchFilter_RunStepsList",
								"converters": [
									{
										"converter": "crt.SearchFilterAttributeConverter",
										"args": [
											"RunStepsList"
										]
									}
								]
							}
						],
						"from": [
							"RunStepsSearchFilter_SearchValue",
							"RunStepsSearchFilter_FilteredColumnsGroups"
						]
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RunLinesExpansionPanel_title",
					"toggleType": "default",
					"togglePosition": "before",
					"expanded": true,
					"labelColor": "auto",
					"fullWidthHeader": true,
					"titleWidth": 20,
					"padding": {
						"top": "small",
						"bottom": "small",
						"left": "none",
						"right": "none"
					},
					"fitContent": true,
					"visible": true,
					"items": [],
					"tools": []
				},
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "RunLinesListContainer",
				"parentName": "RunLinesExpansionPanel",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.GridContainer",
					"columns": [
						"minmax(32px, 1fr)",
						"minmax(32px, 1fr)"
					],
					"rows": "minmax(max-content, 32px)",
					"gap": {
						"columnGap": "large",
						"rowGap": 0
					},
					"styles": {
						"overflow-x": "hidden"
					},
					"items": []
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesList",
				"parentName": "RunLinesListContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.DataGrid",
					"layoutConfig": {
						"colSpan": 2,
						"column": 1,
						"row": 1,
						"rowSpan": 6
					},
					"items": "$RunLinesList",
					"activeRow": "$RunLinesList_ActiveRow",
					"primaryColumnName": "RunLinesListDS_Id",
					"fitContent": true,
					"visible": true,
					"columns": [
						{
							"id": "660abccd-b1d3-499a-a4b9-d5508d936369",
							"code": "RunLinesListDS_LineNumber",
							"caption": "#ResourceString(RunLinesListDS_LineNumber)#",
							"dataValueType": 4,
							"width": 100
						},
						{
							"id": "dde82c1d-89fc-47c7-a2eb-fbf37aa47ed7",
							"code": "RunLinesListDS_Status",
							"caption": "#ResourceString(RunLinesListDS_Status)#",
							"dataValueType": 10,
							"width": 120,
							"referenceSchemaName": "RevenueFeedLineStatus"
						},
						{
							"id": "0c1a40dd-7e8b-48cc-a028-d8d2db0ce652",
							"code": "RunLinesListDS_ErrorCode",
							"caption": "#ResourceString(RunLinesListDS_ErrorCode)#",
							"dataValueType": 10,
							"width": 180,
							"referenceSchemaName": "RevenueFeedErrorCode"
						},
						{
							"id": "9c53ba35-0bc5-4039-8ff6-b98e86f4e424",
							"code": "RunLinesListDS_ErrorMessage",
							"caption": "#ResourceString(RunLinesListDS_ErrorMessage)#",
							"dataValueType": 29,
							"width": 300
						},
						{
							"id": "dae1b294-1d1d-4db1-bf92-2e252cd8bef6",
							"code": "RunLinesListDS_BillingReference",
							"caption": "#ResourceString(RunLinesListDS_BillingReference)#",
							"dataValueType": 28,
							"width": 160
						},
						{
							"id": "ceec07ff-3169-4c12-a85b-f6639894fe4a",
							"code": "RunLinesListDS_AccountCode",
							"caption": "#ResourceString(RunLinesListDS_AccountCode)#",
							"dataValueType": 28,
							"width": 130
						},
						{
							"id": "35bdc714-f899-4b56-94d5-4d9ba2413aaf",
							"code": "RunLinesListDS_Account",
							"caption": "#ResourceString(RunLinesListDS_Account)#",
							"dataValueType": 10,
							"width": 180,
							"referenceSchemaName": "Account"
						},
						{
							"id": "c6c122d4-a255-46bb-acf6-8327b67e9e1b",
							"code": "RunLinesListDS_Opportunity",
							"caption": "#ResourceString(RunLinesListDS_Opportunity)#",
							"dataValueType": 10,
							"width": 180,
							"referenceSchemaName": "Opportunity"
						},
						{
							"id": "e389df90-fa9f-4632-a1d4-89e91701edd3",
							"code": "RunLinesListDS_Amount",
							"caption": "#ResourceString(RunLinesListDS_Amount)#",
							"dataValueType": 6,
							"width": 130
						},
						{
							"id": "4d585758-f87c-41f9-9122-e2c4ecc5084a",
							"code": "RunLinesListDS_PreviousAmount",
							"caption": "#ResourceString(RunLinesListDS_PreviousAmount)#",
							"dataValueType": 6,
							"width": 160
						},
						{
							"id": "3392893d-8187-4b6e-b09c-d4c76008d633",
							"code": "RunLinesListDS_NewAmount",
							"caption": "#ResourceString(RunLinesListDS_NewAmount)#",
							"dataValueType": 6,
							"width": 160
						}
					],
					"features": {
						"rows": {
							"selection": {
								"enable": true,
								"multiple": true
							}
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesToolsContainer",
				"parentName": "RunLinesExpansionPanel",
				"propertyName": "tools",
				"index": 0,
				"values": {
					"type": "crt.GridContainer",
					"rows": "minmax(max-content, 24px)",
					"columns": [
						"minmax(32px, 1fr)"
					],
					"gap": {
						"columnGap": "large",
						"rowGap": "none"
					},
					"color": "transparent",
					"items": []
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesToolsFlexContainer",
				"parentName": "RunLinesToolsContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.FlexContainer",
					"direction": "row",
					"alignItems": "center",
					"gap": "none",
					"items": [],
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesRefreshButton",
				"parentName": "RunLinesToolsFlexContainer",
				"propertyName": "items",
				"index": 0,
				"values": {
					"type": "crt.Button",
					"icon": "reload-icon",
					"iconPosition": "only-icon",
					"color": "default",
					"clicked": {
						"request": "crt.LoadDataRequest",
						"params": {
							"config": {
								"loadType": "reload"
							},
							"dataSourceName": "RunLinesListDS"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesSettingsButton",
				"parentName": "RunLinesToolsFlexContainer",
				"propertyName": "items",
				"index": 1,
				"values": {
					"type": "crt.Button",
					"icon": "actions-button-icon",
					"iconPosition": "only-icon",
					"color": "default",
					"clickMode": "menu",
					"menuItems": []
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesExportMenuItem",
				"parentName": "RunLinesSettingsButton",
				"propertyName": "menuItems",
				"index": 0,
				"values": {
					"type": "crt.MenuItem",
					"icon": "export-button-icon",
					"caption": "#ResourceString(RunLinesExportMenuItem_caption)#",
					"clicked": {
						"request": "crt.ExportDataGridToExcelRequest",
						"params": {
							"viewName": "RunLinesList"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RunLinesSearchFilter",
				"parentName": "RunLinesToolsFlexContainer",
				"propertyName": "items",
				"index": 2,
				"values": {
					"type": "crt.SearchFilter",
					"iconOnly": true,
					"placeholder": "#ResourceString(RunLinesSearchFilter_placeholder)#",
					"_filterOptions": {
						"expose": [
							{
								"attribute": "RunLinesSearchFilter_RunLinesList",
								"converters": [
									{
										"converter": "crt.SearchFilterAttributeConverter",
										"args": [
											"RunLinesList"
										]
									}
								]
							}
						],
						"from": [
							"RunLinesSearchFilter_SearchValue",
							"RunLinesSearchFilter_FilteredColumnsGroups"
						]
					}
				}
			}
		]/**SCHEMA_VIEW_CONFIG_DIFF*/,
		viewModelConfig: /**SCHEMA_VIEW_MODEL_CONFIG*/{
			"attributes": {
				"Name": {
					"modelConfig": {
						"path": "PDS.Name"
					}
				},
				"Id": {
					"modelConfig": {
						"path": "PDS.Id"
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
				"PDS_ScheduledOn": {
					"modelConfig": {
						"path": "PDS.ScheduledOn"
					}
				},
				"PDS_ReceivedOn": {
					"modelConfig": {
						"path": "PDS.ReceivedOn"
					}
				},
				"PDS_StartedOn": {
					"modelConfig": {
						"path": "PDS.StartedOn"
					}
				},
				"PDS_CompletedOn": {
					"modelConfig": {
						"path": "PDS.CompletedOn"
					}
				},
				"PDS_Duration": {
					"modelConfig": {
						"path": "PDS.Duration"
					}
				},
				"PDS_Attempt": {
					"modelConfig": {
						"path": "PDS.Attempt"
					}
				},
				"PDS_FileName": {
					"modelConfig": {
						"path": "PDS.FileName"
					}
				},
				"PDS_FileHash": {
					"modelConfig": {
						"path": "PDS.FileHash"
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
				"PDS_SkippedCount": {
					"modelConfig": {
						"path": "PDS.SkippedCount"
					}
				},
				"PDS_ErrorCode": {
					"modelConfig": {
						"path": "PDS.ErrorCode"
					}
				},
				"PDS_ErrorMessage": {
					"modelConfig": {
						"path": "PDS.ErrorMessage"
					}
				},
				"RunStepsList": {
					"isCollection": true,
					"modelConfig": {
						"path": "RunStepsListDS",
						"sortingConfig": {
							"default": [
								{
									"columnName": "StartedOn",
									"direction": "asc"
								}
							]
						},
						"filterAttributes": [
							{
								"name": "RunStepsSearchFilter_RunStepsList",
								"loadOnChange": true
							}
						]
					},
					"viewModelConfig": {
						"attributes": {
							"RunStepsListDS_Name": {
								"modelConfig": {
									"path": "RunStepsListDS.Name"
								}
							},
							"RunStepsListDS_Result": {
								"modelConfig": {
									"path": "RunStepsListDS.Result"
								}
							},
							"RunStepsListDS_StartedOn": {
								"modelConfig": {
									"path": "RunStepsListDS.StartedOn"
								}
							},
							"RunStepsListDS_Duration": {
								"modelConfig": {
									"path": "RunStepsListDS.Duration"
								}
							},
							"RunStepsListDS_Message": {
								"modelConfig": {
									"path": "RunStepsListDS.Message"
								}
							},
							"RunStepsListDS_Attempt": {
								"modelConfig": {
									"path": "RunStepsListDS.Attempt"
								}
							},
							"RunStepsListDS_Id": {
								"modelConfig": {
									"path": "RunStepsListDS.Id"
								}
							}
						}
					}
				},
				"RunLinesList": {
					"isCollection": true,
					"modelConfig": {
						"path": "RunLinesListDS",
						"sortingConfig": {
							"default": [
								{
									"columnName": "LineNumber",
									"direction": "asc"
								}
							]
						},
						"filterAttributes": [
							{
								"name": "RunLinesSearchFilter_RunLinesList",
								"loadOnChange": true
							}
						]
					},
					"viewModelConfig": {
						"attributes": {
							"RunLinesListDS_LineNumber": {
								"modelConfig": {
									"path": "RunLinesListDS.LineNumber"
								}
							},
							"RunLinesListDS_Status": {
								"modelConfig": {
									"path": "RunLinesListDS.Status"
								}
							},
							"RunLinesListDS_ErrorCode": {
								"modelConfig": {
									"path": "RunLinesListDS.ErrorCode"
								}
							},
							"RunLinesListDS_ErrorMessage": {
								"modelConfig": {
									"path": "RunLinesListDS.ErrorMessage"
								}
							},
							"RunLinesListDS_BillingReference": {
								"modelConfig": {
									"path": "RunLinesListDS.BillingReference"
								}
							},
							"RunLinesListDS_AccountCode": {
								"modelConfig": {
									"path": "RunLinesListDS.AccountCode"
								}
							},
							"RunLinesListDS_Account": {
								"modelConfig": {
									"path": "RunLinesListDS.Account"
								}
							},
							"RunLinesListDS_Opportunity": {
								"modelConfig": {
									"path": "RunLinesListDS.Opportunity"
								}
							},
							"RunLinesListDS_Amount": {
								"modelConfig": {
									"path": "RunLinesListDS.Amount"
								}
							},
							"RunLinesListDS_PreviousAmount": {
								"modelConfig": {
									"path": "RunLinesListDS.PreviousAmount"
								}
							},
							"RunLinesListDS_NewAmount": {
								"modelConfig": {
									"path": "RunLinesListDS.NewAmount"
								}
							},
							"RunLinesListDS_Id": {
								"modelConfig": {
									"path": "RunLinesListDS.Id"
								}
							}
						}
					}
				}
			}
		}/**SCHEMA_VIEW_MODEL_CONFIG*/,
		modelConfig: /**SCHEMA_MODEL_CONFIG*/{
			"dataSources": {
				"PDS": {
					"type": "crt.EntityDataSource",
					"config": {
						"entitySchemaName": "RevenueFeedRun"
					},
					"scope": "page"
				},
				"AttachmentListDS": {
					"type": "crt.EntityDataSource",
					"scope": "viewElement",
					"config": {
						"entitySchemaName": "SysFile",
						"attributes": {
							"Name": {
								"path": "Name"
							}
						}
					}
				},
				"RunStepsListDS": {
					"type": "crt.EntityDataSource",
					"scope": "viewElement",
					"config": {
						"entitySchemaName": "RevenueFeedRunStep",
						"attributes": {
							"Name": {
								"path": "Name"
							},
							"Result": {
								"path": "Result"
							},
							"StartedOn": {
								"path": "StartedOn"
							},
							"Duration": {
								"path": "Duration"
							},
							"Message": {
								"path": "Message"
							},
							"Attempt": {
								"path": "Attempt"
							}
						}
					}
				},
				"RunLinesListDS": {
					"type": "crt.EntityDataSource",
					"scope": "viewElement",
					"config": {
						"entitySchemaName": "RevenueFeedLine",
						"attributes": {
							"LineNumber": {
								"path": "LineNumber"
							},
							"Status": {
								"path": "Status"
							},
							"ErrorCode": {
								"path": "ErrorCode"
							},
							"ErrorMessage": {
								"path": "ErrorMessage"
							},
							"BillingReference": {
								"path": "BillingReference"
							},
							"AccountCode": {
								"path": "AccountCode"
							},
							"Account": {
								"path": "Account"
							},
							"Opportunity": {
								"path": "Opportunity"
							},
							"Amount": {
								"path": "Amount"
							},
							"PreviousAmount": {
								"path": "PreviousAmount"
							},
							"NewAmount": {
								"path": "NewAmount"
							}
						}
					}
				}
			},
			"primaryDataSourceName": "PDS",
			"dependencies": {
				"RunStepsListDS": [
					{
						"attributePath": "Run",
						"relationPath": "PDS.Id"
					}
				],
				"RunLinesListDS": [
					{
						"attributePath": "Run",
						"relationPath": "PDS.Id"
					}
				]
			}
		}/**SCHEMA_MODEL_CONFIG*/,
		handlers: /**SCHEMA_HANDLERS*/[]/**SCHEMA_HANDLERS*/,
		converters: /**SCHEMA_CONVERTERS*/{}/**SCHEMA_CONVERTERS*/,
		validators: /**SCHEMA_VALIDATORS*/{}/**SCHEMA_VALIDATORS*/
	};
});
