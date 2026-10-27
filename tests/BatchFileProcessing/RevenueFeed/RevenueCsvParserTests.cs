using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BatchFileProcessingApp.RevenueFeed;
using ErrorOr;
using FluentAssertions;
using NUnit.Framework;

namespace BatchFileProcessing.Tests.RevenueFeed {

	[TestFixture(Category = "UnitTests")]
	public class RevenueCsvParserTests {

		private const string Header = "BillingReference,AccountCode,OpportunityCode,PeriodStart,PeriodEnd,Amount";

		private static byte[] Csv(params string[] lines) {
			return Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
		}

		[Test]
		public void ReadDataLines_ReturnsDataLinesWithFileLineNumbers() {
			ErrorOr<IReadOnlyList<KeyValuePair<int, string>>> result = RevenueCsvParser.ReadDataLines(
				Csv(Header, "INV-1,ACC-1,OPP-1,2026-09-01,2026-09-30,100.50", "", "INV-2,ACC-1,,2026-09-01,2026-09-30,5"));

			result.IsError.Should().BeFalse();
			result.Value.Select(l => l.Key).Should().Equal(2, 4);
		}

		[Test]
		public void ReadDataLines_AcceptsUtf8Bom() {
			byte[] content = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Csv(Header, "INV-1,ACC-1,,2026-09-01,2026-09-30,1")).ToArray();

			RevenueCsvParser.ReadDataLines(content).IsError.Should().BeFalse();
		}

		[Test]
		public void ReadDataLines_EmptyFile_IsFileEmpty() {
			RevenueCsvParser.ReadDataLines(new byte[0]).FirstError.Code.Should().Be(RevenueFeedErrors.FileEmpty);
			RevenueCsvParser.ReadDataLines(Csv("", "  ")).FirstError.Code.Should().Be(RevenueFeedErrors.FileEmpty);
		}

		[Test]
		public void ReadDataLines_InvalidUtf8_IsFileUnreadable() {
			RevenueCsvParser.ReadDataLines(new byte[] { 0x42, 0xFF, 0xFE, 0x00, 0xC3 }).FirstError.Code
				.Should().Be(RevenueFeedErrors.FileUnreadable);
		}

		[Test]
		public void ReadDataLines_BinaryContent_IsFileUnreadable() {
			RevenueCsvParser.ReadDataLines(new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x00, 0x00 }).FirstError.Code
				.Should().Be(RevenueFeedErrors.FileUnreadable);
		}

		[Test]
		public void ReadDataLines_MissingAndUnknownColumns_IsHeaderMismatchNamingThem() {
			Error error = RevenueCsvParser.ReadDataLines(
				Csv("BillingReference,Account,OpportunityCode,PeriodStart,PeriodEnd,Amount", "x")).FirstError;

			error.Code.Should().Be(RevenueFeedErrors.HeaderMismatch);
			error.Description.Should().Contain("missing AccountCode").And.Contain("\"Account\"");
		}

		[Test]
		public void ReadDataLines_SemicolonDelimitedFile_IsHeaderMismatch() {
			RevenueCsvParser.ReadDataLines(Csv(Header.Replace(',', ';'), "a;b;c;d;e;f")).FirstError.Code
				.Should().Be(RevenueFeedErrors.HeaderMismatch);
		}

		[Test]
		public void ReadDataLines_ColumnsOutOfOrder_IsHeaderMismatch() {
			RevenueCsvParser.ReadDataLines(
				Csv("AccountCode,BillingReference,OpportunityCode,PeriodStart,PeriodEnd,Amount", "x")).FirstError.Code
				.Should().Be(RevenueFeedErrors.HeaderMismatch);
		}

		[Test]
		public void ReadDataLines_HeaderOnly_IsNoDataLines() {
			RevenueCsvParser.ReadDataLines(Csv(Header, "")).FirstError.Code.Should().Be(RevenueFeedErrors.NoDataLines);
		}

		[Test]
		public void ParseLine_ValidLine_ParsesAllValues() {
			ParsedRevenueLine line = RevenueCsvParser.ParseLine(2, "INV-1, ACC-1 ,OPP-1,2026-09-01,2026-09-30,\"1,250.75\"");

			line.IsValid.Should().BeTrue();
			line.BillingReference.Should().Be("INV-1");
			line.AccountCode.Should().Be("ACC-1");
			line.OpportunityCode.Should().Be("OPP-1");
			line.StartDate.Should().Be(new System.DateTime(2026, 9, 1));
			line.EndDate.Should().Be(new System.DateTime(2026, 9, 30));
			line.Amount.Should().Be(1250.75m);
		}

		[TestCase("INV-1,ACC-1,,2026-09-01,2026-09-30", "MissingValue")]
		[TestCase(",ACC-1,,2026-09-01,2026-09-30,1", "MissingValue")]
		[TestCase("INV-1,,,2026-09-01,2026-09-30,1", "MissingValue")]
		[TestCase("INV-1,ACC-1,,2026-09-01,2026-09-30,", "MissingValue")]
		[TestCase("INV-1,ACC-1,,01/09/2026,2026-09-30,1", "InvalidDate")]
		[TestCase("INV-1,ACC-1,,2026-09-30,2026-09-01,1", "InvalidDate")]
		[TestCase("INV-1,ACC-1,,2026-09-01,2026-09-30,12k", "InvalidNumber")]
		[TestCase("INV-1,ACC-1,,2026-09-01,2026-09-30,\"1.000,50\"", "InvalidNumber")]
		public void ParseLine_InvalidLine_IsRejectedWithCode(string raw, string expectedCode) {
			ParsedRevenueLine line = RevenueCsvParser.ParseLine(5, raw);

			line.IsValid.Should().BeFalse();
			line.ErrorCodeId.Should().Be((Guid)typeof(RevenueFeedIds.ErrorCode).GetField(expectedCode,
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null));
			line.ErrorMessage.Should().NotBeNullOrEmpty();
		}

		[Test]
		public void ParseLines_DuplicateBillingReference_RejectsLaterOccurrence() {
			IReadOnlyList<ParsedRevenueLine> lines = RevenueCsvParser.ParseLines(new[] {
				new KeyValuePair<int, string>(2, "INV-1,ACC-1,,2026-09-01,2026-09-30,1"),
				new KeyValuePair<int, string>(3, "inv-1,ACC-2,,2026-09-01,2026-09-30,2")
			});

			lines[0].IsValid.Should().BeTrue();
			lines[1].ErrorCodeId.Should().Be(RevenueFeedIds.ErrorCode.DuplicateInFile);
			lines[1].ErrorMessage.Should().Contain("line 2");
		}

		[Test]
		public void SplitCsvLine_HandlesQuotedCommasAndEscapedQuotes() {
			RevenueCsvParser.SplitCsvLine("a,\"b,c\",\"say \"\"hi\"\"\",").Should().Equal("a", "b,c", "say \"hi\"", "");
		}
	}
}
