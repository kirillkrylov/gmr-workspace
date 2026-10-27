using System;
using System.Collections.Generic;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// <see cref="ErrorOr.Error"/> codes used for file-level failures, and the error-code lookup row each one maps to.
	/// </summary>
	internal static class RevenueFeedErrors {

		internal const string FileMissing = "FileMissing";
		internal const string FileEmpty = "FileEmpty";
		internal const string FileUnreadable = "FileUnreadable";
		internal const string HeaderMismatch = "HeaderMismatch";
		internal const string DuplicateFile = "DuplicateFile";
		internal const string NoDataLines = "NoDataLines";

		private static readonly IReadOnlyDictionary<string, Guid> ErrorCodeIds = new Dictionary<string, Guid> {
			{ FileMissing, RevenueFeedIds.ErrorCode.FileMissing },
			{ FileEmpty, RevenueFeedIds.ErrorCode.FileEmpty },
			{ FileUnreadable, RevenueFeedIds.ErrorCode.FileUnreadable },
			{ HeaderMismatch, RevenueFeedIds.ErrorCode.HeaderMismatch },
			{ DuplicateFile, RevenueFeedIds.ErrorCode.DuplicateFile },
			{ NoDataLines, RevenueFeedIds.ErrorCode.NoDataLines }
		};

		internal static Guid ToErrorCodeId(string code) {
			return ErrorCodeIds.TryGetValue(code ?? string.Empty, out Guid id) ? id : RevenueFeedIds.ErrorCode.UnexpectedError;
		}
	}
}
