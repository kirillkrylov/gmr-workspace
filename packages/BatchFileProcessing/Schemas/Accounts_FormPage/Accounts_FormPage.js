define("Accounts_FormPage", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
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
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 1
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
				"name": "RevenueAccountCode",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 1,
						"rowSpan": 1
					},
					"type": "crt.Input",
					"label": "$Resources.Strings.RevenueAccountCode",
					"control": "$RevenueAccountCode",
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
				"name": "RevenueLastRevenueRun",
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 2,
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
				"index": 2
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
				"parentName": "GeneralInfoTab",
				"propertyName": "items",
				"index": 2
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
							"id": "658939e5-42ac-435c-947d-90965ca5cd1a",
							"code": "RevenueUpdatesListDS_BillingReference",
							"caption": "#ResourceString(RevenueUpdatesListDS_BillingReference)#",
							"dataValueType": 28,
							"width": 160
						},
						{
							"id": "71e97b78-80d1-4ff1-bc0e-377859522acf",
							"code": "RevenueUpdatesListDS_StartDate",
							"caption": "#ResourceString(RevenueUpdatesListDS_StartDate)#",
							"dataValueType": 7,
							"width": 140
						},
						{
							"id": "d79a09a7-ae02-427e-ba79-9d60c2446902",
							"code": "RevenueUpdatesListDS_EndDate",
							"caption": "#ResourceString(RevenueUpdatesListDS_EndDate)#",
							"dataValueType": 7,
							"width": 140
						},
						{
							"id": "b529767b-9c3d-47f9-a39d-783aaf7d50c8",
							"code": "RevenueUpdatesListDS_Amount",
							"caption": "#ResourceString(RevenueUpdatesListDS_Amount)#",
							"dataValueType": 6,
							"width": 130
						},
						{
							"id": "b4b4577a-33fd-4069-9c9a-23b248935dea",
							"code": "RevenueUpdatesListDS_Status",
							"caption": "#ResourceString(RevenueUpdatesListDS_Status)#",
							"dataValueType": 10,
							"width": 120,
							"referenceSchemaName": "RevenueFeedLineStatus"
						},
						{
							"id": "e9105486-7439-49e5-8797-ce9ba95454d7",
							"code": "RevenueUpdatesListDS_Run",
							"caption": "#ResourceString(RevenueUpdatesListDS_Run)#",
							"dataValueType": 10,
							"width": 110,
							"referenceSchemaName": "RevenueFeedRun"
						},
						{
							"id": "bfbdbaf6-70f5-4af2-a461-44cf66c5b466",
							"code": "RevenueUpdatesListDS_AppliedOn",
							"caption": "#ResourceString(RevenueUpdatesListDS_AppliedOn)#",
							"dataValueType": 7,
							"width": 150
						},
						{
							"id": "96d7d36a-2cbb-4c55-ab3f-3c971f5f3ffd",
							"code": "RevenueUpdatesListDS_PreviousAmount",
							"caption": "#ResourceString(RevenueUpdatesListDS_PreviousAmount)#",
							"dataValueType": 6,
							"width": 160
						},
						{
							"id": "ba9c079d-e18d-4a49-9ae8-ddf15952b01f",
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
						"RevenueAccountCode": {
							"modelConfig": {
								"path": "PDS.Code"
							}
						},
						"PDS_ActualAmount": {
							"modelConfig": {
								"path": "PDS.ActualAmount"
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
							"attributePath": "Account",
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
