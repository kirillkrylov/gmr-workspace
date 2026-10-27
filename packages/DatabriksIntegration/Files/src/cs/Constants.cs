namespace DatabriksIntegrationApp {

	/// <summary>
	/// Constants used in the DatabriksIntegrationApp package.
	/// </summary>
	internal static class Constants {
		internal const string LoggerName = "DatabriksIntegrationApp";
		internal const string PackageName = "DatabriksIntegration";

		internal static class SysSettingCodes {
			internal const string ZerobusEndpoint = "DatabricksZerobusEndpoint";
			internal const string WorkspaceUrl = "DatabricksWorkspaceUrl";
			internal const string ClientId = "DatabricksClientId";
			internal const string ClientSecret = "DatabricksClientSecret";
		}

		internal static class SyncObject {
			internal const string SchemaName = "DatabricksSyncObject";
			internal const string EntitySchemaNameColumn = "EntitySchemaName";
			internal const string TargetTableColumn = "TargetTable";
			internal const string IsActiveColumn = "IsActive";
		}

		internal static class Zerobus {
			internal const string ApplicationName = "creatio-databriks-integration";
			internal const string NativeRuntimeVersion = "0.1.0";
			internal const string NativeRuntimeArtifactFileName = "zerobus-0.1.0-runtimes.zip";
			internal const string NativeArtifactsDirectoryName = "native-artifacts";
			internal const string ExternalNativeDirectoryName = "native";
			internal const string RuntimeProductName = "zerobus";
			internal const ulong MaxInflightRequests = 10000;
			internal const ulong FlushTimeoutMs = 10000;
		}

		internal static class Publisher {
			internal const int QueueCapacity = 10000;
			internal const int StopTimeoutSeconds = 5;
			internal const int MaxRetryDelaySeconds = 30;
			internal const int MaxUnexpectedErrorAttempts = 3;
			internal const int NotConfiguredRecheckSeconds = 30;
			internal const int RegistryCacheSeconds = 60;
			internal const int WarningIntervalSeconds = 60;
		}
	}
}
