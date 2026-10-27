using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ErrorOr;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// One data line of the revenue file after parsing. <see cref="ErrorCodeId"/> is set when the line is rejected.
	/// </summary>
	internal sealed class ParsedRevenueLine {
		public int LineNumber { get; set; }
		public string RawText { get; set; }
		public string BillingReference { get; set; }
		public string AccountCode { get; set; }
		public string OpportunityCode { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal? Amount { get; set; }
		public Guid? ErrorCodeId { get; set; }
		public string ErrorMessage { get; set; }
		public bool IsValid => !ErrorCodeId.HasValue;
	}

	/// <summary>
	/// Reads the revenue system's billing CSV. File-level problems come back as errors (the whole run fails);
	/// line-level problems are recorded on the line so that the rest of the file can still be applied.
	/// </summary>
	internal static class RevenueCsvParser {

		internal const string BillingReferenceColumn = "BillingReference";
		internal const string AccountCodeColumn = "AccountCode";
		internal const string OpportunityCodeColumn = "OpportunityCode";
		internal const string PeriodStartColumn = "PeriodStart";
		internal const string PeriodEndColumn = "PeriodEnd";
		internal const string AmountColumn = "Amount";

		internal static readonly string[] ExpectedColumns = {
			BillingReferenceColumn, AccountCodeColumn, OpportunityCodeColumn, PeriodStartColumn, PeriodEndColumn,
			AmountColumn
		};

		private const string DateFormat = "yyyy-MM-dd";

		/// <summary>
		/// Checks the file can be read as UTF-8 CSV with the expected header and at least one data line, and returns its
		/// text lines (header excluded) with their 1-based line numbers in the file.
		/// </summary>
		internal static ErrorOr<IReadOnlyList<KeyValuePair<int, string>>> ReadDataLines(byte[] content) {
			if (content == null || content.Length == 0) {
				return Error.Validation(RevenueFeedErrors.FileEmpty, "The file has no content.");
			}
			string text;
			try {
				text = new UTF8Encoding(false, true).GetString(content);
			} catch (DecoderFallbackException) {
				return Error.Validation(RevenueFeedErrors.FileUnreadable,
					"The file is not valid UTF-8 text; it may be corrupt or not a CSV file.");
			}
			if (text.Length > 0 && text[0] == '﻿') {
				text = text.Substring(1);
			}
			if (text.IndexOf('\0') >= 0) {
				return Error.Validation(RevenueFeedErrors.FileUnreadable,
					"The file contains binary data; it may be corrupt or not a CSV file.");
			}
			string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
			int headerIndex = Array.FindIndex(lines, l => !string.IsNullOrWhiteSpace(l));
			if (headerIndex < 0) {
				return Error.Validation(RevenueFeedErrors.FileEmpty, "The file has no content.");
			}
			ErrorOr<Success> header = CheckHeader(lines[headerIndex]);
			if (header.IsError) {
				return header.Errors;
			}
			var dataLines = new List<KeyValuePair<int, string>>();
			for (int i = headerIndex + 1; i < lines.Length; i++) {
				if (!string.IsNullOrWhiteSpace(lines[i])) {
					dataLines.Add(new KeyValuePair<int, string>(i + 1, lines[i]));
				}
			}
			if (dataLines.Count == 0) {
				return Error.Validation(RevenueFeedErrors.NoDataLines, "The file has a header but no data lines.");
			}
			return dataLines;
		}

		/// <summary>
		/// Parses and validates every data line. Duplicate billing references within the file are all rejected except
		/// the first occurrence.
		/// </summary>
		internal static IReadOnlyList<ParsedRevenueLine> ParseLines(IEnumerable<KeyValuePair<int, string>> dataLines) {
			var result = new List<ParsedRevenueLine>();
			var seenReferences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<int, string> dataLine in dataLines) {
				ParsedRevenueLine line = ParseLine(dataLine.Key, dataLine.Value);
				if (line.IsValid) {
					if (seenReferences.TryGetValue(line.BillingReference, out int firstLine)) {
						Reject(line, RevenueFeedIds.ErrorCode.DuplicateInFile,
							$"Billing reference \"{line.BillingReference}\" already appears on line {firstLine}.");
					} else {
						seenReferences.Add(line.BillingReference, line.LineNumber);
					}
				}
				result.Add(line);
			}
			return result;
		}

		internal static ParsedRevenueLine ParseLine(int lineNumber, string rawText) {
			var line = new ParsedRevenueLine { LineNumber = lineNumber, RawText = rawText };
			List<string> fields = SplitCsvLine(rawText, out bool isQuoteClosed);
			if (!isQuoteClosed) {
				return Reject(line, RevenueFeedIds.ErrorCode.MalformedLine, "A quoted value is not closed; the line may be cut off.");
			}
			if (fields.Count != ExpectedColumns.Length) {
				return Reject(line, RevenueFeedIds.ErrorCode.MissingValue,
					$"Expected {ExpectedColumns.Length} values but found {fields.Count}.");
			}
			line.BillingReference = fields[0];
			line.AccountCode = fields[1];
			line.OpportunityCode = fields[2];
			if (string.IsNullOrEmpty(line.BillingReference)) {
				return Reject(line, RevenueFeedIds.ErrorCode.MissingValue, "Billing reference is empty.");
			}
			if (string.IsNullOrEmpty(line.AccountCode)) {
				return Reject(line, RevenueFeedIds.ErrorCode.MissingValue, "Account code is empty.");
			}
			if (!TryParseDate(fields[3], out DateTime startDate)) {
				return Reject(line, RevenueFeedIds.ErrorCode.InvalidDate,
					$"Period start \"{fields[3]}\" is not a {DateFormat} date.");
			}
			if (!TryParseDate(fields[4], out DateTime endDate)) {
				return Reject(line, RevenueFeedIds.ErrorCode.InvalidDate,
					$"Period end \"{fields[4]}\" is not a {DateFormat} date.");
			}
			line.StartDate = startDate;
			line.EndDate = endDate;
			if (endDate < startDate) {
				return Reject(line, RevenueFeedIds.ErrorCode.InvalidDate, "Period end is before period start.");
			}
			if (string.IsNullOrEmpty(fields[5])) {
				return Reject(line, RevenueFeedIds.ErrorCode.MissingValue, "Amount is empty.");
			}
			if (!decimal.TryParse(fields[5], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount)) {
				return Reject(line, RevenueFeedIds.ErrorCode.InvalidNumber,
					$"Amount \"{fields[5]}\" is not a number (use a dot as the decimal separator).");
			}
			line.Amount = amount;
			return line;
		}

		/// <summary>
		/// Splits one CSV line: comma separated, double-quoted values may contain commas and doubled quotes.
		/// Values are trimmed.
		/// </summary>
		internal static List<string> SplitCsvLine(string line) {
			return SplitCsvLine(line, out bool _);
		}

		/// <summary>
		/// Splits one CSV line and reports whether every opened quote was closed.
		/// </summary>
		internal static List<string> SplitCsvLine(string line, out bool isQuoteClosed) {
			var fields = new List<string>();
			var current = new StringBuilder();
			bool inQuotes = false;
			for (int i = 0; i < line.Length; i++) {
				char c = line[i];
				if (inQuotes) {
					if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') {
						current.Append('"');
						i++;
					} else if (c == '"') {
						inQuotes = false;
					} else {
						current.Append(c);
					}
				} else if (c == '"') {
					inQuotes = true;
				} else if (c == ',') {
					fields.Add(current.ToString().Trim());
					current.Clear();
				} else {
					current.Append(c);
				}
			}
			fields.Add(current.ToString().Trim());
			isQuoteClosed = !inQuotes;
			return fields;
		}

		private static ErrorOr<Success> CheckHeader(string headerLine) {
			List<string> columns = SplitCsvLine(headerLine);
			string[] missing = ExpectedColumns
				.Where(e => !columns.Contains(e, StringComparer.OrdinalIgnoreCase)).ToArray();
			string[] unknown = columns
				.Where(c => !ExpectedColumns.Contains(c, StringComparer.OrdinalIgnoreCase)).ToArray();
			if (missing.Length == 0 && unknown.Length == 0) {
				bool inOrder = columns.Select(c => c.ToUpperInvariant())
					.SequenceEqual(ExpectedColumns.Select(e => e.ToUpperInvariant()));
				if (inOrder) {
					return Result.Success;
				}
				return Error.Validation(RevenueFeedErrors.HeaderMismatch,
					$"Columns must be in this order: {string.Join(",", ExpectedColumns)}.");
			}
			var parts = new List<string>();
			if (missing.Length > 0) {
				parts.Add($"missing {string.Join(", ", missing)}");
			}
			if (unknown.Length > 0) {
				parts.Add($"unexpected {string.Join(", ", unknown.Select(u => $"\"{u}\""))}");
			}
			return Error.Validation(RevenueFeedErrors.HeaderMismatch,
				$"Header row is {string.Join("; ", parts)}. Expected: {string.Join(",", ExpectedColumns)}.");
		}

		private static bool TryParseDate(string value, out DateTime date) {
			return DateTime.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,
				out date);
		}

		private static ParsedRevenueLine Reject(ParsedRevenueLine line, Guid errorCodeId, string message) {
			line.ErrorCodeId = errorCodeId;
			line.ErrorMessage = message;
			return line;
		}
	}
}
