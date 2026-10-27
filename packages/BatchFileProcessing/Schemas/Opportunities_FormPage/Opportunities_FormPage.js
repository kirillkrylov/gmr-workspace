define("Opportunities_FormPage", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
	return {
		viewConfigDiff: /**SCHEMA_VIEW_CONFIG_DIFF*/[
			{
				"operation": "insert",
				"name": "RevenueExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RevenueExpansionPanel_title",
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
				"parentName": "OverviewTab",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "RevenueExpansionPanelGrid",
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
				"parentName": "RevenueExpansionPanel",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RevenueOpportunityAmount",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.RevenueOpportunityAmount",
					"control": "$RevenueOpportunityAmount",
					"labelPosition": "auto"
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 0
			},
			{
				"operation": "insert",
				"name": "RevenueActualAmount",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_ActualAmount",
					"control": "$PDS_ActualAmount",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 1
			},
			{
				"operation": "insert",
				"name": "RevenueAmountVariance",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_AmountVariance",
					"control": "$PDS_AmountVariance",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 2
			},
			{
				"operation": "insert",
				"name": "RevenueAmountVariancePercent",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 2,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.NumberInput",
					"label": "$Resources.Strings.PDS_AmountVariancePercent",
					"control": "$PDS_AmountVariancePercent",
					"labelPosition": "auto",
					"readonly": true
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 3
			},
			{
				"operation": "insert",
				"name": "RevenueSystemCode",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.PDS_RevenueSystemCode",
					"control": "$PDS_RevenueSystemCode",
					"labelPosition": "auto"
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "RevenueLastRevenueRun",
				"values": {
					"layoutConfig": {
						"column": 2,
						"row": 3,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.ComboBox",
					"label": "$Resources.Strings.PDS_LastRevenueRun",
					"control": "$PDS_LastRevenueRun",
					"labelPosition": "auto",
					"mode": "List",
					"showValueAsLink": true,
					"readonly": true
				},
				"parentName": "RevenueExpansionPanelGrid",
				"propertyName": "items",
				"index": 5
			},
			{
				"operation": "insert",
				"name": "RevenueUpdatesExpansionPanel",
				"values": {
					"type": "crt.ExpansionPanel",
					"title": "$Resources.Strings.RevenueUpdatesExpansionPanel_title",
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
				"parentName": "OverviewTab",
				"propertyName": "items",
				"index": 4
			},
			{
				"operation": "insert",
				"name": "RevenueUpdatesListContainer",
				"parentName": "RevenueUpdatesExpansionPanel",
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
				"name": "RevenueUpdatesList",
				"parentName": "RevenueUpdatesListContainer",
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
					"items": "$RevenueUpdatesList",
					"activeRow": "$RevenueUpdatesList_ActiveRow",
					"primaryColumnName": "RevenueUpdatesListDS_Id",
					"fitContent": true,
					"visible": true,
					"columns": [
						{
							"id": "ad87d626-0d83-49c3-bd3e-b894d301e313",
							"code": "RevenueUpdatesListDS_BillingReference",
							"caption": "#ResourceString(RevenueUpdatesListDS_BillingReference)#",
							"dataValueType": 28,
							"width": 160
						},
						{
							"id": "4acc0951-94db-49d9-ae41-e4b758715498",
							"code": "RevenueUpdatesListDS_StartDate",
							"caption": "#ResourceString(RevenueUpdatesListDS_StartDate)#",
							"dataValueType": 7,
							"width": 140
						},
						{
							"id": "0f4ef35c-a4fe-4697-818d-52c7782cade6",
							"code": "RevenueUpdatesListDS_EndDate",
							"caption": "#ResourceString(RevenueUpdatesListDS_EndDate)#",
							"dataValueType": 7,
							"width": 140
						},
						{
							"id": "1ee09325-8f43-4e69-a8b7-f46a473dbc3e",
							"code": "RevenueUpdatesListDS_Amount",
							"caption": "#ResourceString(RevenueUpdatesListDS_Amount)#",
							"dataValueType": 6,
							"width": 130
						},
						{
							"id": "27e664a6-9693-45e7-9060-371997215179",
							"code": "RevenueUpdatesListDS_Status",
							"caption": "#ResourceString(RevenueUpdatesListDS_Status)#",
							"dataValueType": 10,
							"width": 120,
							"referenceSchemaName": "RevenueFeedLineStatus"
						},
						{
							"id": "c28fc2c3-7d05-4831-b688-ef0e625e970e",
							"code": "RevenueUpdatesListDS_Run",
							"caption": "#ResourceString(RevenueUpdatesListDS_Run)#",
							"dataValueType": 10,
							"width": 110,
							"referenceSchemaName": "RevenueFeedRun"
						},
						{
							"id": "36ee3331-7025-48df-90fc-22103298de05",
							"code": "RevenueUpdatesListDS_AppliedOn",
							"caption": "#ResourceString(RevenueUpdatesListDS_AppliedOn)#",
							"dataValueType": 7,
							"width": 150
						},
						{
							"id": "f124c0f8-5857-4cf5-a936-72bd9448adfb",
							"code": "RevenueUpdatesListDS_PreviousAmount",
							"caption": "#ResourceString(RevenueUpdatesListDS_PreviousAmount)#",
							"dataValueType": 6,
							"width": 160
						},
						{
							"id": "0a8b7497-a245-45b9-87d2-3042431a3d6e",
							"code": "RevenueUpdatesListDS_NewAmount",
							"caption": "#ResourceString(RevenueUpdatesListDS_NewAmount)#",
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
				"name": "RevenueUpdatesToolsContainer",
				"parentName": "RevenueUpdatesExpansionPanel",
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
				"name": "RevenueUpdatesToolsFlexContainer",
				"parentName": "RevenueUpdatesToolsContainer",
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
				"name": "RevenueUpdatesRefreshButton",
				"parentName": "RevenueUpdatesToolsFlexContainer",
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
							"dataSourceName": "RevenueUpdatesListDS"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RevenueUpdatesSettingsButton",
				"parentName": "RevenueUpdatesToolsFlexContainer",
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
				"name": "RevenueUpdatesExportMenuItem",
				"parentName": "RevenueUpdatesSettingsButton",
				"propertyName": "menuItems",
				"index": 0,
				"values": {
					"type": "crt.MenuItem",
					"icon": "export-button-icon",
					"caption": "#ResourceString(RevenueUpdatesExportMenuItem_caption)#",
					"clicked": {
						"request": "crt.ExportDataGridToExcelRequest",
						"params": {
							"viewName": "RevenueUpdatesList"
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "RevenueUpdatesSearchFilter",
				"parentName": "RevenueUpdatesToolsFlexContainer",
				"propertyName": "items",
				"index": 2,
				"values": {
					"type": "crt.SearchFilter",
					"iconOnly": true,
					"placeholder": "#ResourceString(RevenueUpdatesSearchFilter_placeholder)#",
					"_filterOptions": {
						"expose": [
							{
								"attribute": "RevenueUpdatesSearchFilter_RevenueUpdatesList",
								"converters": [
									{
										"converter": "crt.SearchFilterAttributeConverter",
										"args": [
											"RevenueUpdatesList"
										]
									}
								]
							}
						],
						"from": [
							"RevenueUpdatesSearchFilter_SearchValue",
							"RevenueUpdatesSearchFilter_FilteredColumnsGroups"
						]
					}
				}
			}
		]/**SCHEMA_VIEW_CONFIG_DIFF*/,
		viewModelConfigDiff: /**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [],
				"values": {
					"attributes": {
						"RevenueOpportunityAmount": {
							"modelConfig": {
								"path": "PDS.Amount"
							}
						},
						"PDS_ActualAmount": {
							"modelConfig": {
								"path": "PDS.ActualAmount"
							}
						},
						"PDS_AmountVariance": {
							"modelConfig": {
								"path": "PDS.AmountVariance"
							}
						},
						"PDS_AmountVariancePercent": {
							"modelConfig": {
								"path": "PDS.AmountVariancePercent"
							}
						},
						"PDS_RevenueSystemCode": {
							"modelConfig": {
								"path": "PDS.RevenueSystemCode"
							}
						},
						"PDS_LastRevenueRun": {
							"modelConfig": {
								"path": "PDS.LastRevenueRun"
							}
						},
						"RevenueUpdatesList": {
							"isCollection": true,
							"modelConfig": {
								"path": "RevenueUpdatesListDS",
								"sortingConfig": {
									"default": [
										{
											"columnName": "AppliedOn",
											"direction": "desc"
										}
									]
								},
								"filterAttributes": [
									{
										"name": "RevenueUpdatesSearchFilter_RevenueUpdatesList",
										"loadOnChange": true
									}
								]
							},
							"viewModelConfig": {
								"attributes": {
									"RevenueUpdatesListDS_BillingReference": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.BillingReference"
										}
									},
									"RevenueUpdatesListDS_StartDate": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.StartDate"
										}
									},
									"RevenueUpdatesListDS_EndDate": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.EndDate"
										}
									},
									"RevenueUpdatesListDS_Amount": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.Amount"
										}
									},
									"RevenueUpdatesListDS_Status": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.Status"
										}
									},
									"RevenueUpdatesListDS_Run": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.Run"
										}
									},
									"RevenueUpdatesListDS_AppliedOn": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.AppliedOn"
										}
									},
									"RevenueUpdatesListDS_PreviousAmount": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.PreviousAmount"
										}
									},
									"RevenueUpdatesListDS_NewAmount": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.NewAmount"
										}
									},
									"RevenueUpdatesListDS_Id": {
										"modelConfig": {
											"path": "RevenueUpdatesListDS.Id"
										}
									}
								}
							}
						}
					}
				}
			}
		]/**SCHEMA_VIEW_MODEL_CONFIG_DIFF*/,
		modelConfigDiff: /**SCHEMA_MODEL_CONFIG_DIFF*/[
			{
				"operation": "merge",
				"path": [
					"dataSources"
				],
				"values": {
					"RevenueUpdatesListDS": {
						"type": "crt.EntityDataSource",
						"scope": "viewElement",
						"config": {
							"entitySchemaName": "RevenueFeedLine",
							"attributes": {
								"BillingReference": {
									"path": "BillingReference"
								},
								"StartDate": {
									"path": "StartDate"
								},
								"EndDate": {
									"path": "EndDate"
								},
								"Amount": {
									"path": "Amount"
								},
								"Status": {
									"path": "Status"
								},
								"Run": {
									"path": "Run"
								},
								"AppliedOn": {
									"path": "AppliedOn"
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
				}
			},
			{
				"operation": "merge",
				"path": [
					"dependencies"
				],
				"values": {
					"RevenueUpdatesListDS": [
						{
							"attributePath": "Opportunity",
							"relationPath": "PDS.Id"
						}
					]
				}
			}
		]/**SCHEMA_MODEL_CONFIG_DIFF*/,
		handlers: /**SCHEMA_HANDLERS*/[]/**SCHEMA_HANDLERS*/,
		converters: /**SCHEMA_CONVERTERS*/{}/**SCHEMA_CONVERTERS*/,
		validators: /**SCHEMA_VALIDATORS*/{}/**SCHEMA_VALIDATORS*/
	};
});
