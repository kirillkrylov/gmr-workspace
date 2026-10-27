using System;
using System.Text.Json;
using DatabriksIntegrationApp.Sync;
using FluentAssertions;
using NUnit.Framework;

namespace DatabriksIntegration.Tests.Sync {

	[TestFixture(Category = "UnitTests")]
	public class ChangeRecordTests {
		private static readonly Guid RecordId = new Guid("11111111-2222-3333-4444-555555555555");
		private static readonly Guid UserId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
		private static readonly DateTime ChangedOn = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Utc);

		private static ChangeRecord CreateRecord(ChangeOperation operation, params ColumnChange[] columns) {
			return new ChangeRecord {
				TargetTable = "workspace.default.creatio_changes",
				EntitySchemaName = "Contact",
				RecordId = RecordId,
				Operation = operation,
				ChangedOnUtc = ChangedOn,
				ChangedById = UserId,
				Columns = columns
			};
		}

		private static JsonElement Parse(string json) {
			return JsonDocument.Parse(json).RootElement;
		}

		[Test]
		public void ToJson_WritesChangeLogColumns() {
			JsonElement row = Parse(CreateRecord(ChangeOperation.Insert).ToJson());

			row.GetProperty("entity").GetString().Should().Be("Contact");
			row.GetProperty("record_id").GetString().Should().Be(RecordId.ToString("D"));
			row.GetProperty("operation").GetString().Should().Be("insert");
			row.GetProperty("changed_on").GetInt64().Should().Be(ChangeRecord.ToEpochMicroseconds(ChangedOn));
			row.GetProperty("changed_by").GetString().Should().Be(UserId.ToString("D"));
			Parse(row.GetProperty("payload").GetString()).GetProperty("columns").EnumerateObject().Should().BeEmpty();
		}

		[Test]
		public void ToJson_EmptyUser_WritesNullChangedBy() {
			ChangeRecord record = CreateRecord(ChangeOperation.Delete);
			record.ChangedById = Guid.Empty;

			Parse(record.ToJson()).GetProperty("changed_by").ValueKind.Should().Be(JsonValueKind.Null);
		}

		[Test]
		public void ToJson_Update_WritesOldAndNewValues() {
			string json = CreateRecord(ChangeOperation.Update, new ColumnChange("Name", "Old", "New")).ToJson();

			JsonElement name = Parse(Parse(json).GetProperty("payload").GetString()).GetProperty("columns")
				.GetProperty("Name");
			name.GetProperty("old").GetString().Should().Be("Old");
			name.GetProperty("new").GetString().Should().Be("New");
		}

		[Test]
		public void ToJson_Insert_WritesNewValueOnly() {
			string json = CreateRecord(ChangeOperation.Insert, new ColumnChange("Name", null, "New")).ToJson();

			JsonElement name = Parse(Parse(json).GetProperty("payload").GetString()).GetProperty("columns")
				.GetProperty("Name");
			name.TryGetProperty("old", out _).Should().BeFalse();
			name.GetProperty("new").GetString().Should().Be("New");
		}

		[Test]
		public void ToJson_FormatsValueTypes() {
			Guid ownerId = Guid.NewGuid();
			var birthDate = new DateTime(1990, 5, 1, 0, 0, 0, DateTimeKind.Utc);
			string json = CreateRecord(ChangeOperation.Insert,
				new ColumnChange("OwnerId", null, ownerId),
				new ColumnChange("BirthDate", null, birthDate),
				new ColumnChange("DoNotUseEmail", null, true),
				new ColumnChange("Age", null, 36),
				new ColumnChange("Amount", null, 12.5m),
				new ColumnChange("Notes", null, DBNull.Value)).ToJson();

			JsonElement columns = Parse(Parse(json).GetProperty("payload").GetString()).GetProperty("columns");
			columns.GetProperty("OwnerId").GetProperty("new").GetString().Should().Be(ownerId.ToString("D"));
			columns.GetProperty("BirthDate").GetProperty("new").GetString().Should().Be(birthDate.ToString("o"));
			columns.GetProperty("DoNotUseEmail").GetProperty("new").GetBoolean().Should().BeTrue();
			columns.GetProperty("Age").GetProperty("new").GetInt32().Should().Be(36);
			columns.GetProperty("Amount").GetProperty("new").GetDecimal().Should().Be(12.5m);
			columns.GetProperty("Notes").GetProperty("new").ValueKind.Should().Be(JsonValueKind.Null);
		}

		[Test]
		public void ToJson_SkipsBinaryColumns() {
			string json = CreateRecord(ChangeOperation.Update,
				new ColumnChange("Photo", null, new byte[] { 1, 2, 3 }),
				new ColumnChange("Name", "a", "b")).ToJson();

			JsonElement columns = Parse(Parse(json).GetProperty("payload").GetString()).GetProperty("columns");
			columns.TryGetProperty("Photo", out _).Should().BeFalse();
			columns.TryGetProperty("Name", out _).Should().BeTrue();
		}

		[Test]
		public void ToEpochMicroseconds_ConvertsUtc() {
			ChangeRecord.ToEpochMicroseconds(new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc))
				.Should().Be(1000000);
		}
	}
}
