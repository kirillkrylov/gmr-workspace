define("RevenueFeedLine_FormPage", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
	return {
		viewConfigDiff: /**SCHEMA_VIEW_CONFIG_DIFF*/[
			{
				"operation": "insert",
				"name": "LineName",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_Name",
					"control": "$PDS_Name",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "SideAreaProfileContainer",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineStatus",
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
				"name": "LineRun",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_Run",
					"control": "$PDS_Run",
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
				"operation": "insert",
				"name": "LineLineNumber",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 4,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_LineNumber",
					"control": "$PDS_LineNumber",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "SideAreaProfileContainer",
				"propertyName": "items",
				"index": 3
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
				"name": "LineDetailsExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.LineDetailsExpansionPanel_title",
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
				"name": "LineDetailsExpansionPanelGrid",
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
				"parentName": "LineDetailsExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineBillingReference",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_BillingReference",
					"control": "$PDS_BillingReference",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineAppliedOn",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_AppliedOn",
					"control": "$PDS_AppliedOn",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "LineAccountCode",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_AccountCode",
					"control": "$PDS_AccountCode",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "LineOpportunityCode",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_OpportunityCode",
					"control": "$PDS_OpportunityCode",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "LineStartDate",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_StartDate",
					"control": "$PDS_StartDate",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "LineEndDate",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.DateTimePicker",
					"label": "$Resources.Strings.PDS_EndDate",
					"control": "$PDS_EndDate",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "LineAmount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 4,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_Amount",
					"control": "$PDS_Amount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineDetailsExpansionPanelGrid",
				"propertyName": "items",
				"index": 6
			},
			{
				"operation": "insert",
				"name": "LineResultExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.LineResultExpansionPanel_title",
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
				"name": "LineResultExpansionPanelGrid",
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
				"parentName": "LineResultExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineAccount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_Account",
					"control": "$PDS_Account",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "LineResultExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineOpportunity",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_Opportunity",
					"control": "$PDS_Opportunity",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "LineResultExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "LinePreviousAmount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_PreviousAmount",
					"control": "$PDS_PreviousAmount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineResultExpansionPanelGrid",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "LineNewAmount",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_NewAmount",
					"control": "$PDS_NewAmount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "LineResultExpansionPanelGrid",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "LineErrorExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.LineErrorExpansionPanel_title",
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
				"index": 3
			},
			{
				"operation": "insert",
				"name": "LineErrorExpansionPanelGrid",
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
				"parentName": "LineErrorExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineErrorCode",
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
				"parentName": "LineErrorExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineErrorMessage",
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
				"parentName": "LineErrorExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "LineRawExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.LineRawExpansionPanel_title",
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
				"index": 4
			},
			{
				"operation": "insert",
				"name": "LineRawExpansionPanelGrid",
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
				"parentName": "LineRawExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "LineRawText",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 2,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_RawText",
					"control": "$PDS_RawText",
					"labelPosition": "auto",
					"multiline": true,
					"readonly": true
				},
				"parentName": "LineRawExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			}
		]/**SCHEMA_VIEW_CONFIG_DIFF*/,
		viewModelConfigDiff: /**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [],
				"values": {
					"attributes": {
						"PDS_Name": {
							"modelConfig": {
								"path": "PDS.Name"
							}
						},
						"PDS_Status": {
							"modelConfig": {
								"path": "PDS.Status"
							}
						},
						"PDS_Run": {
							"modelConfig": {
								"path": "PDS.Run"
							}
						},
						"PDS_LineNumber": {
							"modelConfig": {
								"path": "PDS.LineNumber"
							}
						},
						"PDS_BillingReference": {
							"modelConfig": {
								"path": "PDS.BillingReference"
							}
						},
						"PDS_AppliedOn": {
							"modelConfig": {
								"path": "PDS.AppliedOn"
							}
						},
						"PDS_AccountCode": {
							"modelConfig": {
								"path": "PDS.AccountCode"
							}
						},
						"PDS_OpportunityCode": {
							"modelConfig": {
								"path": "PDS.OpportunityCode"
							}
						},
						"PDS_StartDate": {
							"modelConfig": {
								"path": "PDS.StartDate"
							}
						},
						"PDS_EndDate": {
							"modelConfig": {
								"path": "PDS.EndDate"
							}
						},
						"PDS_Amount": {
							"modelConfig": {
								"path": "PDS.Amount"
							}
						},
						"PDS_Account": {
							"modelConfig": {
								"path": "PDS.Account"
							}
						},
						"PDS_Opportunity": {
							"modelConfig": {
								"path": "PDS.Opportunity"
							}
						},
						"PDS_PreviousAmount": {
							"modelConfig": {
								"path": "PDS.PreviousAmount"
							}
						},
						"PDS_NewAmount": {
							"modelConfig": {
								"path": "PDS.NewAmount"
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
						"PDS_RawText": {
							"modelConfig": {
								"path": "PDS.RawText"
							}
						}
					}
				}
			}
		]/**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/,
		modelConfigDiff: /**SCHEMA_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [],
				"values": {
					"primaryDataSourceName": "PDS"
				}
			},
			{
				"operation": "merge",
				"path": [
					"dataSources"
				],
				"values": {
					"PDS": {
						"type": "crt.EntityDataSource",
						"scope": "page",
						"config": {
							"entitySchemaName": "RevenueFeedLine"
						}
					}
				}
			}
		]/**SCHEMA_MODEL_CONFIG_DIFF*/,
		handlers: /**SCHEMA_HANDLERS*/[]/**SCHEMA_HANDLERS*/,
		converters: /**SCHEMA_CONVERTERS*/{}/**SCHEMA_CONVERTERS*/,
		validators: /**SCHEMA_VALIDATORS*/{}/**SCHEMA_VALIDATORS*/
	};
});
