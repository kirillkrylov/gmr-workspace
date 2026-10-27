using DatabriksIntegrationApp.Zerobus;
using FluentAssertions;
using NUnit.Framework;

namespace DatabriksIntegration.Tests.Zerobus {

	[TestFixture(Category = "UnitTests")]
	public class DatabricksOptionsTests {

		private static DatabricksOptions CreateOptions() {
			return new DatabricksOptions {
				ZerobusEndpoint = "https://1.zerobus.example.com",
				WorkspaceUrl = "https://dbc.example.com",
				ClientId = "client",
				ClientSecret = "top-secret"
			};
		}

		[Test]
		public void IsConfigured_AllValuesSet_ReturnsTrue() {
			CreateOptions().IsConfigured.Should().BeTrue();
		}

		[TestCase("ZerobusEndpoint")]
		[TestCase("WorkspaceUrl")]
		[TestCase("ClientId")]
		[TestCase("ClientSecret")]
		public void IsConfigured_MissingValue_ReturnsFalse(string property) {
			DatabricksOptions options = CreateOptions();
			typeof(DatabricksOptions).GetProperty(property,
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				.SetValue(options, " ");

			options.IsConfigured.Should().BeFalse();
		}

		[Test]
		public void ToStringAndConnectionKey_NeverContainSecret() {
			DatabricksOptions options = CreateOptions();

			options.ToString().Should().NotContain("top-secret");
			options.ConnectionKey.Should().NotContain("top-secret");
		}
	}
}
