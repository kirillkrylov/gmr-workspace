using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace DatabriksIntegrationApp.Sync {

	internal enum ChangeOperation {
		Insert,
		Update,
		Delete
	}

	/// <summary>
	/// One changed column: its value before and after the save.
	/// </summary>
	internal sealed class ColumnChange {
		internal ColumnChange(string name, object oldValue, object newValue) {
			Name = name;
			OldValue = oldValue;
			NewValue = newValue;
		}

		internal string Name { get; }
		internal object OldValue { get; }
		internal object NewValue { get; }
	}

	/// <summary>
	/// A change of one Creatio record, destined for one Databricks table.
	/// </summary>
	internal sealed class ChangeRecord {
		internal string TargetTable { get; set; }
		internal string EntitySchemaName { get; set; }
		internal Guid RecordId { get; set; }
		internal ChangeOperation Operation { get; set; }
		internal DateTime ChangedOnUtc { get; set; }
		internal Guid ChangedById { get; set; }
		internal IReadOnlyList<ColumnChange> Columns { get; set; } = Array.Empty<ColumnChange>();

		/// <summary>
		/// Serializes the record as one JSON row of the generic change-log table:
		/// entity, record_id, operation, changed_on (epoch microseconds), changed_by, payload (JSON text).
		/// </summary>
		internal string ToJson() {
			using (var buffer = new MemoryStream())
			using (var writer = new Utf8JsonWriter(buffer)) {
				writer.WriteStartObject();
				writer.WriteString("entity", EntitySchemaName);
				writer.WriteString("record_id", RecordId.ToString("D"));
				writer.WriteString("operation", Operation.ToString().ToLowerInvariant());
				writer.WriteNumber("changed_on", ToEpochMicroseconds(ChangedOnUtc));
				if (ChangedById == Guid.Empty) {
					writer.WriteNull("changed_by");
				} else {
					writer.WriteString("changed_by", ChangedById.ToString("D"));
				}
				writer.WriteString("payload", SerializePayload());
				writer.WriteEndObject();
				writer.Flush();
				return Encoding.UTF8.GetString(buffer.ToArray());
			}
		}

		private string SerializePayload() {
			using (var buffer = new MemoryStream())
			using (var writer = new Utf8JsonWriter(buffer)) {
				writer.WriteStartObject();
				writer.WriteStartObject("columns");
				foreach (ColumnChange column in Columns) {
					if (IsBinary(column.NewValue) || IsBinary(column.OldValue)) {
						continue;
					}
					writer.WriteStartObject(column.Name);
					if (Operation == ChangeOperation.Update) {
						WriteValue(writer, "old", column.OldValue);
					}
					WriteValue(writer, "new", column.NewValue);
					writer.WriteEndObject();
				}
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.Flush();
				return Encoding.UTF8.GetString(buffer.ToArray());
			}
		}

		internal static long ToEpochMicroseconds(DateTime utc) {
			DateTime value = utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : utc;
			return (value.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / 10;
		}

		private static bool IsBinary(object value) {
			return value is byte[] || value is Stream;
		}

		private static void WriteValue(Utf8JsonWriter writer, string name, object value) {
			switch (value) {
				case null:
				case DBNull _:
					writer.WriteNull(name);
					break;
				case string text:
					writer.WriteString(name, text);
					break;
				case Guid guid:
					writer.WriteString(name, guid.ToString("D"));
					break;
				case bool flag:
					writer.WriteBoolean(name, flag);
					break;
				case DateTime dateTime:
					writer.WriteString(name, dateTime.ToString("o", CultureInfo.InvariantCulture));
					break;
				case int number:
					writer.WriteNumber(name, number);
					break;
				case long number:
					writer.WriteNumber(name, number);
					break;
				case decimal number:
					writer.WriteNumber(name, number);
					break;
				case double number:
					writer.WriteNumber(name, number);
					break;
				case float number:
					writer.WriteNumber(name, number);
					break;
				default:
					writer.WriteString(name, Convert.ToString(value, CultureInfo.InvariantCulture));
					break;
			}
		}
	}
}
