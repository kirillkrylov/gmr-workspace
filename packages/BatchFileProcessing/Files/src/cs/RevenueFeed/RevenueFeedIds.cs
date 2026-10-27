using System;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// Ids of the lookup rows the package ships in its Data bindings. The pipeline refers to them by id, never by name.
	/// </summary>
	internal static class RevenueFeedIds {

		internal static class RunStatus {
			internal static readonly Guid WaitingForFile = new Guid("39143f82-b72f-4174-8943-b34b8d93c9f9");
			internal static readonly Guid Processing = new Guid("5c6d3294-903c-460d-9c09-ed4f0aa0af95");
			internal static readonly Guid Succeeded = new Guid("0ff0ff5a-3125-4ca4-9fd6-b2d87685a337");
			internal static readonly Guid CompletedWithErrors = new Guid("2de11088-06d6-43f7-88a9-961d3af52820");
			internal static readonly Guid Failed = new Guid("51484354-c49c-4495-baa6-ee61c687b70b");
			internal static readonly Guid Missed = new Guid("3a5aec1f-394a-49c8-a4d6-0fab862be839");
			internal static readonly Guid Skipped = new Guid("c5863961-60de-4909-8621-f009cbd818d6");
		}

		internal static class Stage {
			internal static readonly Guid ReceiveFile = new Guid("81a50fc8-a44c-459e-a802-febfc27700f7");
			internal static readonly Guid ValidateFile = new Guid("59587228-2ad7-4cd6-9df1-72cbe272c6fe");
			internal static readonly Guid ParseLines = new Guid("1f4a165a-2052-4132-a3b0-4c46cff8998b");
			internal static readonly Guid MatchRecords = new Guid("0538e30c-8908-4032-a12f-b533fb7e2263");
			internal static readonly Guid ApplyAmounts = new Guid("6472ed44-66d9-43bf-a850-38953ace22f0");
			internal static readonly Guid UpdateTotals = new Guid("6f3447ef-f28c-47e9-8802-76b8755e33a0");
			internal static readonly Guid CompleteRun = new Guid("58a02987-cb61-4586-826a-b5fd94681e61");
		}

		internal static class StepResult {
			internal static readonly Guid Succeeded = new Guid("3228cfa3-ed4b-4e79-bec7-3818c5baa783");
			internal static readonly Guid CompletedWithWarnings = new Guid("7e415e25-d33f-4021-b54f-a39fd02ade1d");
			internal static readonly Guid Failed = new Guid("1a1e041d-f8d0-4cdc-9cd5-5e4d048fcad2");
		}

		internal static class LineStatus {
			internal static readonly Guid Pending = new Guid("78e5af5e-cfd6-48d3-b938-8e28726fe77a");
			internal static readonly Guid Applied = new Guid("44cc5baa-df37-4a05-a4ba-e594b6d7d704");
			internal static readonly Guid Rejected = new Guid("5d6a77ad-77e7-45f7-a2e5-40832c0e425d");
			internal static readonly Guid Skipped = new Guid("edb843d7-29f5-40e4-88d8-21f1eb954104");
		}

		internal static class ErrorCode {
			internal static readonly Guid FileMissing = new Guid("669836b8-8a40-4df8-a9ca-b9eef5f12c27");
			internal static readonly Guid FileEmpty = new Guid("70615dbb-b563-45fc-9274-0bdf20bcd423");
			internal static readonly Guid FileUnreadable = new Guid("cc67b546-7b15-477b-a677-f6369ba043c9");
			internal static readonly Guid HeaderMismatch = new Guid("33d498d9-5ebe-4cb8-bfd1-881c18bc2112");
			internal static readonly Guid DuplicateFile = new Guid("8c0083db-b3be-4343-9544-ffed07692ac8");
			internal static readonly Guid NoDataLines = new Guid("d10515eb-0d53-43e9-8901-415f16571588");
			internal static readonly Guid MissingValue = new Guid("bc3f23a6-e8c3-4c62-8759-ebafa1a6eb53");
			internal static readonly Guid InvalidNumber = new Guid("7a9d8dca-9791-40ac-a9c1-408263dd7297");
			internal static readonly Guid InvalidDate = new Guid("ba809d2e-f424-42fd-8370-e1a8ec1b5b02");
			internal static readonly Guid DuplicateInFile = new Guid("1b452c65-5947-41be-81a0-a27f8f501239");
			internal static readonly Guid AccountNotFound = new Guid("9c3a07a6-c6c0-4aed-8ef1-6e3351aa8c29");
			internal static readonly Guid OpportunityNotFound = new Guid("d4ab5a3c-31ef-4011-8201-e3659d7f1a8f");
			internal static readonly Guid OpportunityAccountMismatch = new Guid("f363f5be-10b6-47dd-8aa2-fb4eae7a55a9");
			internal static readonly Guid AlreadyApplied = new Guid("34e929b0-cec8-4340-87c4-bcc30213e4f6");
			internal static readonly Guid UnexpectedError = new Guid("361987fa-8768-4d25-89ee-d0dc4602ee08");
			internal static readonly Guid FeedMissed = new Guid("91bbd94b-585f-4374-a841-a6128e06a49b");
			internal static readonly Guid MalformedLine = new Guid("9a6d2838-7a70-42e3-b027-cbd96a91ce29");
		}

		/// <summary>
		/// Process element names of the RevenueFeedRun_ProcessFile process, mapped to the stage each one runs.
		/// </summary>
		internal static Guid? StageForElement(string elementName) {
			switch (elementName) {
				case "ReceiveFile": return Stage.ReceiveFile;
				case "ValidateFile": return Stage.ValidateFile;
				case "ParseLines": return Stage.ParseLines;
				case "MatchRecords": return Stage.MatchRecords;
				case "ApplyAmounts": return Stage.ApplyAmounts;
				case "UpdateTotals": return Stage.UpdateTotals;
				case "CompleteRun": return Stage.CompleteRun;
				default: return null;
			}
		}
	}
}
