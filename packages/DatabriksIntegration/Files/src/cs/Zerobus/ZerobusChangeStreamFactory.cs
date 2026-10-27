using System;
using System.Diagnostics.CodeAnalysis;
using Databricks.Zerobus;
using ErrorOr;

namespace DatabriksIntegrationApp.Zerobus {

	/// <summary>
	/// A JSON ingest stream to one Databricks table.
	/// </summary>
	internal interface IChangeStream : IDisposable {
		/// <summary>Queues one JSON record; returns its offset.</summary>
		long Ingest(string json);

		/// <summary>Flushes queued records and closes the stream.</summary>
		void Close();
	}

	internal interface IChangeStreamFactory : IDisposable {
		ErrorOr<IChangeStream> Create(DatabricksOptions options, string tableName);
	}

	// Thin adapter over the native SDK; covered by the fork's own tests and the live run.
	[ExcludeFromCodeCoverage]
	internal sealed class ZerobusChangeStreamFactory : IChangeStreamFactory {
		private readonly object _syncRoot = new object();
		private readonly IZerobusNativeLibraryLoader _nativeLibraryLoader;
		private ZerobusSdk _sdk;
		private string _sdkConnectionKey;

		public ZerobusChangeStreamFactory(IZerobusNativeLibraryLoader nativeLibraryLoader) {
			_nativeLibraryLoader = nativeLibraryLoader;
		}

		public ErrorOr<IChangeStream> Create(DatabricksOptions options, string tableName) {
			ErrorOr<string> native = _nativeLibraryLoader.EnsureLoaded();
			if (native.IsError) {
				return native.Errors;
			}
			ZerobusSdk sdk = GetSdk(options);
			StreamConfigurationOptions streamOptions = StreamConfigurationOptionsFactory.Create(
				maxInflightRequests: Constants.Zerobus.MaxInflightRequests,
				flushTimeoutMs: Constants.Zerobus.FlushTimeoutMs);
			JsonZerobusStream stream = sdk.CreateJsonStream(tableName, options.ClientId, options.ClientSecret,
				streamOptions);
			return new JsonChangeStream(stream);
		}

		private ZerobusSdk GetSdk(DatabricksOptions options) {
			lock (_syncRoot) {
				if (_sdk != null && _sdkConnectionKey == options.ConnectionKey) {
					return _sdk;
				}
				_sdk?.Dispose();
				_sdk = ZerobusSdk.CreateBuilder()
					.Endpoint(options.ZerobusEndpoint)
					.UnityCatalogUrl(options.WorkspaceUrl)
					.ApplicationName(Constants.Zerobus.ApplicationName)
					.Build();
				_sdkConnectionKey = options.ConnectionKey;
				return _sdk;
			}
		}

		public void Dispose() {
			lock (_syncRoot) {
				_sdk?.Dispose();
				_sdk = null;
			}
		}

		private sealed class JsonChangeStream : IChangeStream {
			private readonly JsonZerobusStream _stream;

			internal JsonChangeStream(JsonZerobusStream stream) {
				_stream = stream;
			}

			public long Ingest(string json) => _stream.IngestRecord(json);

			public void Close() => _stream.Close();

			public void Dispose() => _stream.Dispose();
		}
	}
}
