define("RevenueFeedDashboard", /**SCHEMA_DEPS*/[]/**SCHEMA_DEPS*/, function/**SCHEMA_ARGS*/()/**SCHEMA_ARGS*/ {
	return {
		viewConfigDiff: /**SCHEMA_VIEW_CONFIG_DIFF*/[
			{
				"operation": "insert",
				"name": "IndicatorWidget_TodaySucceeded",
				"parentName": "Main",
				"propertyName": "items",
				"index": 0,
				"values": {
					"layoutConfig": {
						"column": 1,
						"row": 1,
						"colSpan": 3,
						"rowSpan": 3
					},
					"type": "crt.IndicatorWidget",
					"config": {
						"title": "#ResourceString(IndicatorWidget_TodaySucceeded_title)#",
						"data": {
							"providing": {
								"attribute": "IndicatorWidget_TodaySucceeded_Data",
								"schemaName": "RevenueFeedRun",
								"filters": {
									"filter": {
										"items": {
											"today": {
												"filterType": 1,
												"comparisonType": 3,
												"isEnabled": true,
												"trimDateTimeParameterToDate": true,
												"dataValueType": 7,
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "ScheduledOn"
												},
												"rightExpression": {
													"expressionType": 1,
													"functionType": 1,
													"macrosType": 4
												}
											},
											"status": {
												"filterType": 4,
												"comparisonType": 3,
												"isEnabled": true,
												"dataValueType": 10,
												"referenceSchemaName": "RevenueFeedRunStatus",
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "Status"
												},
												"rightExpressions": [
													{
														"expressionType": 2,
														"parameter": {
															"dataValueType": 10,
															"value": {
																"Id": "0ff0ff5a-3125-4ca4-9fd6-b2d87685a337",
																"value": "0ff0ff5a-3125-4ca4-9fd6-b2d87685a337",
																"displayValue": "Succeeded",
																"Name": "Succeeded"
															}
														}
													}
												]
											}
										},
										"logicalOperation": 0,
										"isEnabled": true,
										"filterType": 6,
										"rootSchemaName": "RevenueFeedRun"
									},
									"filterAttributes": []
								},
								"aggregation": {
									"column": {
										"orderDirection": 0,
										"orderPosition": -1,
										"isVisible": true,
										"expression": {
											"expressionType": 1,
											"functionArgument": {
												"expressionType": 0,
												"columnPath": "Id"
											},
											"functionType": 2,
											"aggregationType": 1,
											"aggregationEvalType": 2
										}
									}
								},
								"dependencies": [
									{
										"attributePath": "Id",
										"relationPath": "DashboardDS.Id"
									}
								]
							},
							"formatting": {
								"type": "number",
								"decimalSeparator": ".",
								"decimalPrecision": 0,
								"thousandSeparator": ","
							}
						},
						"text": {
							"template": "#ResourceString(IndicatorWidget_TodaySucceeded_template)#",
							"metricMacros": "{0}",
							"fontSizeMode": "medium",
							"labelPosition": "above-under"
						},
						"layout": {
							"color": "green",
							"icon": {
								"iconName": "checkmark-filled-icon"
							}
						},
						"theme": "without-fill"
					},
					"visible": true
				}
			},
			{
				"operation": "insert",
				"name": "IndicatorWidget_TodayCompletedwitherrors",
				"parentName": "Main",
				"propertyName": "items",
				"index": 1,
				"values": {
					"layoutConfig": {
						"column": 4,
						"row": 1,
						"colSpan": 3,
						"rowSpan": 3
					},
					"type": "crt.IndicatorWidget",
					"config": {
						"title": "#ResourceString(IndicatorWidget_TodayCompletedwitherrors_title)#",
						"data": {
							"providing": {
								"attribute": "IndicatorWidget_TodayCompletedwitherrors_Data",
								"schemaName": "RevenueFeedRun",
								"filters": {
									"filter": {
										"items": {
											"today": {
												"filterType": 1,
												"comparisonType": 3,
												"isEnabled": true,
												"trimDateTimeParameterToDate": true,
												"dataValueType": 7,
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "ScheduledOn"
												},
												"rightExpression": {
													"expressionType": 1,
													"functionType": 1,
													"macrosType": 4
												}
											},
											"status": {
												"filterType": 4,
												"comparisonType": 3,
												"isEnabled": true,
												"dataValueType": 10,
												"referenceSchemaName": "RevenueFeedRunStatus",
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "Status"
												},
												"rightExpressions": [
													{
														"expressionType": 2,
														"parameter": {
															"dataValueType": 10,
															"value": {
																"Id": "2de11088-06d6-43f7-88a9-961d3af52820",
																"value": "2de11088-06d6-43f7-88a9-961d3af52820",
																"displayValue": "Completed with errors",
																"Name": "Completed with errors"
															}
														}
													}
												]
											}
										},
										"logicalOperation": 0,
										"isEnabled": true,
										"filterType": 6,
										"rootSchemaName": "RevenueFeedRun"
									},
									"filterAttributes": []
								},
								"aggregation": {
									"column": {
										"orderDirection": 0,
										"orderPosition": -1,
										"isVisible": true,
										"expression": {
											"expressionType": 1,
											"functionArgument": {
												"expressionType": 0,
												"columnPath": "Id"
											},
											"functionType": 2,
											"aggregationType": 1,
											"aggregationEvalType": 2
										}
									}
								},
								"dependencies": [
									{
										"attributePath": "Id",
										"relationPath": "DashboardDS.Id"
									}
								]
							},
							"formatting": {
								"type": "number",
								"decimalSeparator": ".",
								"decimalPrecision": 0,
								"thousandSeparator": ","
							}
						},
						"text": {
							"template": "#ResourceString(IndicatorWidget_TodayCompletedwitherrors_template)#",
							"metricMacros": "{0}",
							"fontSizeMode": "medium",
							"labelPosition": "above-under"
						},
						"layout": {
							"color": "orange",
							"icon": {
								"iconName": "warning-icon"
							}
						},
						"theme": "without-fill"
					},
					"visible": true
				}
			},
			{
				"operation": "insert",
				"name": "IndicatorWidget_TodayFailed",
				"parentName": "Main",
				"propertyName": "items",
				"index": 2,
				"values": {
					"layoutConfig": {
						"column": 7,
						"row": 1,
						"colSpan": 3,
						"rowSpan": 3
					},
					"type": "crt.IndicatorWidget",
					"config": {
						"title": "#ResourceString(IndicatorWidget_TodayFailed_title)#",
						"data": {
							"providing": {
								"attribute": "IndicatorWidget_TodayFailed_Data",
								"schemaName": "RevenueFeedRun",
								"filters": {
									"filter": {
										"items": {
											"today": {
												"filterType": 1,
												"comparisonType": 3,
												"isEnabled": true,
												"trimDateTimeParameterToDate": true,
												"dataValueType": 7,
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "ScheduledOn"
												},
												"rightExpression": {
													"expressionType": 1,
													"functionType": 1,
													"macrosType": 4
												}
											},
											"status": {
												"filterType": 4,
												"comparisonType": 3,
												"isEnabled": true,
												"dataValueType": 10,
												"referenceSchemaName": "RevenueFeedRunStatus",
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "Status"
												},
												"rightExpressions": [
													{
														"expressionType": 2,
														"parameter": {
															"dataValueType": 10,
															"value": {
																"Id": "51484354-c49c-4495-baa6-ee61c687b70b",
																"value": "51484354-c49c-4495-baa6-ee61c687b70b",
																"displayValue": "Failed",
																"Name": "Failed"
															}
														}
													}
												]
											}
										},
										"logicalOperation": 0,
										"isEnabled": true,
										"filterType": 6,
										"rootSchemaName": "RevenueFeedRun"
									},
									"filterAttributes": []
								},
								"aggregation": {
									"column": {
										"orderDirection": 0,
										"orderPosition": -1,
										"isVisible": true,
										"expression": {
											"expressionType": 1,
											"functionArgument": {
												"expressionType": 0,
												"columnPath": "Id"
											},
											"functionType": 2,
											"aggregationType": 1,
											"aggregationEvalType": 2
										}
									}
								},
								"dependencies": [
									{
										"attributePath": "Id",
										"relationPath": "DashboardDS.Id"
									}
								]
							},
							"formatting": {
								"type": "number",
								"decimalSeparator": ".",
								"decimalPrecision": 0,
								"thousandSeparator": ","
							}
						},
						"text": {
							"template": "#ResourceString(IndicatorWidget_TodayFailed_template)#",
							"metricMacros": "{0}",
							"fontSizeMode": "medium",
							"labelPosition": "above-under"
						},
						"layout": {
							"color": "red",
							"icon": {
								"iconName": "exclamation-filled-icon"
							}
						},
						"theme": "without-fill"
					},
					"visible": true
				}
			},
			{
				"operation": "insert",
				"name": "IndicatorWidget_TodayMissed",
				"parentName": "Main",
				"propertyName": "items",
				"index": 3,
				"values": {
					"layoutConfig": {
						"column": 10,
						"row": 1,
						"colSpan": 3,
						"rowSpan": 3
					},
					"type": "crt.IndicatorWidget",
					"config": {
						"title": "#ResourceString(IndicatorWidget_TodayMissed_title)#",
						"data": {
							"providing": {
								"attribute": "IndicatorWidget_TodayMissed_Data",
								"schemaName": "RevenueFeedRun",
								"filters": {
									"filter": {
										"items": {
											"today": {
												"filterType": 1,
												"comparisonType": 3,
												"isEnabled": true,
												"trimDateTimeParameterToDate": true,
												"dataValueType": 7,
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "ScheduledOn"
												},
												"rightExpression": {
													"expressionType": 1,
													"functionType": 1,
													"macrosType": 4
												}
											},
											"status": {
												"filterType": 4,
												"comparisonType": 3,
												"isEnabled": true,
												"dataValueType": 10,
												"referenceSchemaName": "RevenueFeedRunStatus",
												"leftExpression": {
													"expressionType": 0,
													"columnPath": "Status"
												},
												"rightExpressions": [
													{
														"expressionType": 2,
														"parameter": {
															"dataValueType": 10,
															"value": {
																"Id": "3a5aec1f-394a-49c8-a4d6-0fab862be839",
																"value": "3a5aec1f-394a-49c8-a4d6-0fab862be839",
																"displayValue": "Missed",
																"Name": "Missed"
															}
														}
													}
												]
											}
										},
										"logicalOperation": 0,
										"isEnabled": true,
										"filterType": 6,
										"rootSchemaName": "RevenueFeedRun"
									},
									"filterAttributes": []
								},
								"aggregation": {
									"column": {
										"orderDirection": 0,
										"orderPosition": -1,
										"isVisible": true,
										"expression": {
											"expressionType": 1,
											"functionArgument": {
												"expressionType": 0,
												"columnPath": "Id"
											},
											"functionType": 2,
											"aggregationType": 1,
											"aggregationEvalType": 2
										}
									}
								},
								"dependencies": [
									{
										"attributePath": "Id",
										"relationPath": "DashboardDS.Id"
									}
								]
							},
							"formatting": {
								"type": "number",
								"decimalSeparator": ".",
								"decimalPrecision": 0,
								"thousandSeparator": ","
							}
						},
						"text": {
							"template": "#ResourceString(IndicatorWidget_TodayMissed_template)#",
							"metricMacros": "{0}",
							"fontSizeMode": "medium",
							"labelPosition": "above-under"
						},
						"layout": {
							"color": "coral",
							"icon": {
								"iconName": "clock-icon"
							}
						},
						"theme": "without-fill"
					},
					"visible": true
				}
			},
			{
				"operation": "insert",
				"name": "RunsByStatusChart",
				"parentName": "Main",
				"propertyName": "items",
				"index": 4,
				"values": {
					"layoutConfig": {
						"column": 9,
						"row": 4,
						"colSpan": 4,
						"rowSpan": 9
					},
					"type": "crt.ChartWidget",
					"config": {
						"title": "#ResourceString(RunsByStatusChart_title)#",
						"color": "dark-blue",
						"theme": "without-fill",
						"series": [
							{
								"type": "doughnut",
								"label": "#ResourceString(RunsByStatusChart_series_0)#",
								"legend": {
									"enabled": true
								},
								"dataLabel": {
									"display": true
								},
								"data": {
									"providing": {
										"attribute": "RunsByStatusChart_SeriesData_8315f61",
										"schemaName": "RevenueFeedRun",
										"filters": {
											"filter": {
												"items": {
													"last7days": {
														"items": {
															"today": {
																"filterType": 1,
																"comparisonType": 3,
																"isEnabled": true,
																"trimDateTimeParameterToDate": true,
																"dataValueType": 7,
																"leftExpression": {
																	"expressionType": 0,
																	"columnPath": "ScheduledOn"
																},
																"rightExpression": {
																	"expressionType": 1,
																	"functionType": 1,
																	"macrosType": 4
																}
															},
															"previous6days": {
																"filterType": 1,
																"comparisonType": 3,
																"isEnabled": true,
																"trimDateTimeParameterToDate": true,
																"dataValueType": 7,
																"leftExpression": {
																	"expressionType": 0,
																	"columnPath": "ScheduledOn"
																},
																"rightExpression": {
																	"expressionType": 1,
																	"functionType": 1,
																	"macrosType": 25,
																	"functionArgument": {
																		"expressionType": 2,
																		"parameter": {
																			"dataValueType": 4,
																			"value": 6
																		}
																	}
																}
															}
														},
														"logicalOperation": 1,
														"isEnabled": true,
														"filterType": 6,
														"rootSchemaName": "RevenueFeedRun"
													}
												},
												"logicalOperation": 0,
												"isEnabled": true,
												"filterType": 6,
												"rootSchemaName": "RevenueFeedRun"
											},
											"filterAttributes": []
										},
										"aggregation": {
											"column": {
												"orderDirection": 0,
												"orderPosition": -1,
												"isVisible": true,
												"expression": {
													"expressionType": 1,
													"functionArgument": {
														"expressionType": 0,
														"columnPath": "Id"
													},
													"functionType": 2,
													"aggregationType": 1,
													"aggregationEvalType": 2
												}
											}
										},
										"grouping": {
											"type": "by-value",
											"column": {
												"orderDirection": 0,
												"orderPosition": -1,
												"isVisible": true,
												"expression": {
													"expressionType": 0,
													"columnPath": "Status"
												}
											}
										},
										"rowCount": 50,
										"dependencies": [
											{
												"attributePath": "Id",
												"relationPath": "DashboardDS.Id"
											}
										]
									}
								}
							}
						]
					}
				}
			},
			{
				"operation": "insert",
				"name": "ListWidget_TodayRuns",
				"parentName": "Main",
				"propertyName": "items",
				"index": 5,
				"values": {
					"type": "crt.ListWidget",
					"title": "#ResourceString(ListWidget_TodayRuns_title)#",
					"widgetConfig": {
						"theme": "without-fill",
						"layout": {
							"color": "dark-blue"
						}
					},
					"items": "$ListWidget_TodayRuns",
					"activeRow": "$ListWidget_TodayRuns_activeRow",
					"primaryColumnName": "ListWidget_TodayRunsDS_Id",
					"columns": [
						{
							"id": "4bed5326-0b91-4351-84d7-564d40b2c6dc",
							"code": "ListWidget_TodayRunsDS_ScheduledOn",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_ScheduledOn)#",
							"dataValueType": 7,
							"width": 150
						},
						{
							"id": "b679740a-045d-45ff-ba52-5669d9132025",
							"code": "ListWidget_TodayRunsDS_Name",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_Name)#",
							"dataValueType": 28,
							"width": 110
						},
						{
							"id": "cd4bbc31-f891-4cad-821f-94e178f3d404",
							"code": "ListWidget_TodayRunsDS_Status",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_Status)#",
							"dataValueType": 10,
							"width": 160,
							"referenceSchemaName": "RevenueFeedRunStatus"
						},
						{
							"id": "dd30c6b5-be9d-4e2c-8b23-1bc892a89984",
							"code": "ListWidget_TodayRunsDS_ErrorCode",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_ErrorCode)#",
							"dataValueType": 10,
							"width": 200,
							"referenceSchemaName": "RevenueFeedErrorCode"
						},
						{
							"id": "6efb7d8b-e890-4579-8a5d-4cf27cef0ae3",
							"code": "ListWidget_TodayRunsDS_AppliedCount",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_AppliedCount)#",
							"dataValueType": 4,
							"width": 90
						},
						{
							"id": "d4bbc4ea-c843-4c2c-bfa7-d9114e58df0b",
							"code": "ListWidget_TodayRunsDS_RejectedCount",
							"caption": "#ResourceString(ListWidget_TodayRunsDS_RejectedCount)#",
							"dataValueType": 4,
							"width": 90
						}
					],
					"layoutConfig": {
						"column": 1,
						"row": 4,
						"colSpan": 8,
						"rowSpan": 9
					},
					"fitContent": true,
					"features": {
						"rows": {
							"selection": {
								"enable": false
							}
						}
					}
				}
			},
			{
				"operation": "insert",
				"name": "ListWidget_RejectedLines",
				"parentName": "Main",
				"propertyName": "items",
				"index": 6,
				"values": {
					"type": "crt.ListWidget",
					"title": "#ResourceString(ListWidget_RejectedLines_title)#",
					"widgetConfig": {
						"theme": "without-fill",
						"layout": {
							"color": "dark-blue"
						}
					},
					"items": "$ListWidget_RejectedLines",
					"activeRow": "$ListWidget_RejectedLines_activeRow",
					"primaryColumnName": "ListWidget_RejectedLinesDS_Id",
					"columns": [
						{
							"id": "b009dc7b-dad2-4d2b-8c82-0c6fb329752a",
							"code": "ListWidget_RejectedLinesDS_Run",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_Run)#",
							"dataValueType": 10,
							"width": 110,
							"referenceSchemaName": "RevenueFeedRun"
						},
						{
							"id": "e1636c07-056d-4e74-9a64-4b4174d414e8",
							"code": "ListWidget_RejectedLinesDS_LineNumber",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_LineNumber)#",
							"dataValueType": 4,
							"width": 90
						},
						{
							"id": "a7b864c7-996e-4044-856b-0aad99651976",
							"code": "ListWidget_RejectedLinesDS_BillingReference",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_BillingReference)#",
							"dataValueType": 28,
							"width": 150
						},
						{
							"id": "acabf9be-579d-482f-806b-f5f667841107",
							"code": "ListWidget_RejectedLinesDS_ErrorCode",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_ErrorCode)#",
							"dataValueType": 10,
							"width": 200,
							"referenceSchemaName": "RevenueFeedErrorCode"
						},
						{
							"id": "10e4da6a-1b7f-4053-94ca-2f4fac8d930c",
							"code": "ListWidget_RejectedLinesDS_ErrorMessage",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_ErrorMessage)#",
							"dataValueType": 29,
							"width": 380
						},
						{
							"id": "0dd5a92d-3c96-4e18-809e-1296c48e2eab",
							"code": "ListWidget_RejectedLinesDS_CreatedOn",
							"caption": "#ResourceString(ListWidget_RejectedLinesDS_CreatedOn)#",
							"dataValueType": 7,
							"width": 150
						}
					],
					"layoutConfig": {
						"column": 1,
						"row": 13,
						"colSpan": 12,
						"rowSpan": 9
					},
					"fitContent": true,
					"features": {
						"rows": {
							"selection": {
								"enable": false
							}
						}
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
						"ListWidget_TodayRuns": {
							"isCollection": true,
							"modelConfig": {
								"path": "ListWidget_TodayRunsDS",
								"sortingConfig": {
									"default": [
										{
											"columnName": "ScheduledOn",
											"direction": "asc"
										}
									]
								},
								"filterAttributes": [
									{
										"name": "ListWidget_TodayRuns_PredefinedFilter",
										"loadOnChange": true
									}
								]
							},
							"viewModelConfig": {
								"attributes": {
									"ListWidget_TodayRunsDS_ScheduledOn": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.ScheduledOn"
										}
									},
									"ListWidget_TodayRunsDS_Name": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.Name"
										}
									},
									"ListWidget_TodayRunsDS_Status": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.Status"
										}
									},
									"ListWidget_TodayRunsDS_ErrorCode": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.ErrorCode"
										}
									},
									"ListWidget_TodayRunsDS_AppliedCount": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.AppliedCount"
										}
									},
									"ListWidget_TodayRunsDS_RejectedCount": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.RejectedCount"
										}
									},
									"ListWidget_TodayRunsDS_Id": {
										"modelConfig": {
											"path": "ListWidget_TodayRunsDS.Id"
										}
									}
								}
							}
						},
						"ListWidget_TodayRuns_PredefinedFilter": {
							"value": {
								"items": {
									"today": {
										"filterType": 1,
										"comparisonType": 3,
										"isEnabled": true,
										"trimDateTimeParameterToDate": true,
										"dataValueType": 7,
										"leftExpression": {
											"expressionType": 0,
											"columnPath": "ScheduledOn"
										},
										"rightExpression": {
											"expressionType": 1,
											"functionType": 1,
											"macrosType": 4
										}
									}
								},
								"logicalOperation": 0,
								"isEnabled": true,
								"filterType": 6,
								"rootSchemaName": "RevenueFeedRun"
							}
						},
						"ListWidget_RejectedLines": {
							"isCollection": true,
							"modelConfig": {
								"path": "ListWidget_RejectedLinesDS",
								"sortingConfig": {
									"default": [
										{
											"columnName": "CreatedOn",
											"direction": "desc"
										}
									]
								},
								"filterAttributes": [
									{
										"name": "ListWidget_RejectedLines_PredefinedFilter",
										"loadOnChange": true
									}
								]
							},
							"viewModelConfig": {
								"attributes": {
									"ListWidget_RejectedLinesDS_Run": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.Run"
										}
									},
									"ListWidget_RejectedLinesDS_LineNumber": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.LineNumber"
										}
									},
									"ListWidget_RejectedLinesDS_BillingReference": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.BillingReference"
										}
									},
									"ListWidget_RejectedLinesDS_ErrorCode": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.ErrorCode"
										}
									},
									"ListWidget_RejectedLinesDS_ErrorMessage": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.ErrorMessage"
										}
									},
									"ListWidget_RejectedLinesDS_CreatedOn": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.CreatedOn"
										}
									},
									"ListWidget_RejectedLinesDS_Id": {
										"modelConfig": {
											"path": "ListWidget_RejectedLinesDS.Id"
										}
									}
								}
							}
						},
						"ListWidget_RejectedLines_PredefinedFilter": {
							"value": {
								"items": {
									"rejected": {
										"filterType": 4,
										"comparisonType": 3,
										"isEnabled": true,
										"dataValueType": 10,
										"referenceSchemaName": "RevenueFeedLineStatus",
										"leftExpression": {
											"expressionType": 0,
											"columnPath": "Status"
										},
										"rightExpressions": [
											{
												"expressionType": 2,
												"parameter": {
													"dataValueType": 10,
													"value": {
														"Id": "5d6a77ad-77e7-45f7-a2e5-40832c0e425d",
														"value": "5d6a77ad-77e7-45f7-a2e5-40832c0e425d",
														"displayValue": "Rejected",
														"Name": "Rejected"
													}
												}
											}
										]
									}
								},
								"logicalOperation": 0,
								"isEnabled": true,
								"filterType": 6,
								"rootSchemaName": "RevenueFeedLine"
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
					"dataSources": {
						"ListWidget_TodayRunsDS": {
							"type": "crt.EntityDataSource",
							"scope": "viewElement",
							"config": {
								"entitySchemaName": "RevenueFeedRun",
								"attributes": {
									"ScheduledOn": {
										"path": "ScheduledOn"
									},
									"Name": {
										"path": "Name"
									},
									"Status": {
										"path": "Status"
									},
									"ErrorCode": {
										"path": "ErrorCode"
									},
									"AppliedCount": {
										"path": "AppliedCount"
									},
									"RejectedCount": {
										"path": "RejectedCount"
									}
								}
							}
						},
						"ListWidget_RejectedLinesDS": {
							"type": "crt.EntityDataSource",
							"scope": "viewElement",
							"config": {
								"entitySchemaName": "RevenueFeedLine",
								"attributes": {
									"Run": {
										"path": "Run"
									},
									"LineNumber": {
										"path": "LineNumber"
									},
									"BillingReference": {
										"path": "BillingReference"
									},
									"ErrorCode": {
										"path": "ErrorCode"
									},
									"ErrorMessage": {
										"path": "ErrorMessage"
									},
									"CreatedOn": {
										"path": "CreatedOn"
									}
								}
							}
						}
					},
					"dependencies": {
						"ListWidget_TodayRunsDS": [
							{
								"attributePath": "Id",
								"relationPath": "DashboardDS.Id"
							}
						],
						"ListWidget_RejectedLinesDS": [
							{
								"attributePath": "Run",
								"relationPath": "DashboardDS.Id"
							}
						]
					}
				}
			}
		]/**SCHEMA_MODEL_CONFIG_DIFF*/,
		handlers: /**SCHEMA_HANDLERS*/[]/**SCHEMA_HANDLERS*/,
		converters: /**SCHEMA_CONVERTERS*/{}/**SCHEMA_CONVERTERS*/,
		validators: /**SCHEMA_VALIDATORS*/{}/**SCHEMA_VALIDATORS*/
	};
});
