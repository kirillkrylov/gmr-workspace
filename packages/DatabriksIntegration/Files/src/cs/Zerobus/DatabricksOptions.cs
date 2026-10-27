using Terrasoft.Core;
using Terrasoft.Core.Configuration;

namespace DatabriksIntegrationApp.Zerobus {

	/// <summary>
	/// Connection settings for Databricks Zerobus, read from system settings.
	/// </summary>
	internal sealed class DatabricksOptions {

		internal string ZerobusEndpoint { get; set; }
		internal string WorkspaceUrl { get; set; }
		internal string ClientId { get; set; }
		internal string ClientSecret { get; set; }

		internal bool IsConfigured =>
			!string.IsNullOrWhiteSpace(ZerobusEndpoint) &&
			!string.IsNullOrWhiteSpace(WorkspaceUrl) &&
			!string.IsNullOrWhiteSpace(ClientId) &&
			!string.IsNullOrWhiteSpace(ClientSecret);

		/// <summary>
		/// Identifies the SDK connection; excludes the secret so it can be logged and compared.
		/// </summary>
		internal string ConnectionKey => ZerobusEndpoint + "|" + WorkspaceUrl + "|" + ClientId;

		public override string ToString() {
			return $"Zerobus endpoint '{ZerobusEndpoint}', workspace '{WorkspaceUrl}', client '{ClientId}'";
		}
	}

	internal interface IDatabricksOptionsProvider {
		DatabricksOptions Get(UserConnection userConnection);
	}

	internal sealed class SysSettingsDatabricksOptionsProvider : IDatabricksOptionsProvider {

		public DatabricksOptions Get(UserConnection userConnection) {
			return new DatabricksOptions {
				ZerobusEndpoint = Read(userConnection, Constants.SysSettingCodes.ZerobusEndpoint),
				WorkspaceUrl = Read(userConnection, Constants.SysSettingCodes.WorkspaceUrl),
				ClientId = Read(userConnection, Constants.SysSettingCodes.ClientId),
				ClientSecret = Read(userConnection, Constants.SysSettingCodes.ClientSecret)
			};
		}

		private static string Read(UserConnection userConnection, string code) {
			return SysSettings.TryGetValue(userConnection, code, out object value)
				? (value as string ?? string.Empty).Trim()
				: string.Empty;
		}
	}
}
