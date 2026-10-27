using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Databricks.Zerobus;
using DatabriksIntegrationApp.Zerobus;
using ErrorOr;
using Terrasoft.Core;

namespace DatabriksIntegrationApp.Sync {

	/// <summary>
	/// Sends change records to Databricks Zerobus from a single background worker.
	/// </summary>
	/// <remarks>
	/// Delivery is best effort and at most once across process restarts: records wait in a bounded in-memory
	/// queue, and the queue drops new records (with a logged warning) when full.
	/// </remarks>
	internal interface IChangePublisher {
		/// <summary>Queues a record without blocking; false when it was dropped.</summary>
		bool Enqueue(ChangeRecord record, UserConnection userConnection);

		void Start();

		void Stop();
	}

	internal sealed class ChangePublisher : IChangePublisher, IDisposable {
		private readonly object _syncRoot = new object();
		private readonly BlockingCollection<ChangeRecord> _queue =
			new BlockingCollection<ChangeRecord>(Constants.Publisher.QueueCapacity);
		private readonly Dictionary<string, IChangeStream> _streams =
			new Dictionary<string, IChangeStream>(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<string, DateTime> _lastWarnings =
			new ConcurrentDictionary<string, DateTime>();
		private readonly IDatabricksOptionsProvider _optionsProvider;
		private readonly IChangeStreamFactory _streamFactory;
		private readonly Func<DateTime> _utcNow;
		private readonly ILog _log;
		private volatile AppConnection _appConnection;
		private Thread _worker;
		private CancellationTokenSource _cancellation;
		private string _streamsOptionsKey;

		public ChangePublisher(IDatabricksOptionsProvider optionsProvider, IChangeStreamFactory streamFactory,
				Func<DateTime> utcNow, ILog log) {
			_optionsProvider = optionsProvider;
			_streamFactory = streamFactory;
			_utcNow = utcNow;
			_log = log;
		}

		/// <summary>Waits between delivery retries; returns false when cancelled. Replaced in tests.</summary>
		internal Func<TimeSpan, CancellationToken, bool> Delay { get; set; } =
			(delay, token) => !token.WaitHandle.WaitOne(delay);

		/// <summary>Starts the worker on first use when the application listener has not.</summary>
		internal bool AutoStart { get; set; } = true;

		internal int PendingCount => _queue.Count;

		public bool Enqueue(ChangeRecord record, UserConnection userConnection) {
			if (_appConnection == null && userConnection?.AppConnection != null) {
				_appConnection = userConnection.AppConnection;
			}
			if (!_queue.TryAdd(record)) {
				WarnRateLimited("queue-full", "Databricks change queue is full ({0} records); dropping changes.",
					Constants.Publisher.QueueCapacity);
				return false;
			}
			if (AutoStart) {
				Start();
			}
			return true;
		}

		public void Start() {
			lock (_syncRoot) {
				if (_worker != null) {
					if (_cancellation != null && _cancellation.IsCancellationRequested) {
						_log.Warn("Databricks publisher start refused: prior worker still stopping.");
					}
					return;
				}
				var cancellation = new CancellationTokenSource();
				var worker = new Thread(() => Run(cancellation.Token)) {
					IsBackground = true,
					Name = "DatabriksIntegration.ZerobusPublisher"
				};
				_cancellation = cancellation;
				_worker = worker;
				worker.Start();
			}
		}

		public void Stop() {
			Thread worker;
			lock (_syncRoot) {
				worker = _worker;
				if (worker == null) {
					return;
				}
				_cancellation.Cancel();
			}
			if (!worker.Join(TimeSpan.FromSeconds(Constants.Publisher.StopTimeoutSeconds))) {
				_log.WarnFormat("Databricks publisher did not stop within {0} seconds; {1} queued change(s) not sent.",
					Constants.Publisher.StopTimeoutSeconds, _queue.Count);
			}
		}

		public void Dispose() {
			Stop();
		}

		/// <summary>Delivers every queued record on the calling thread. Used by the worker and by tests.</summary>
		internal void DeliverPending(CancellationToken cancellationToken) {
			while (!cancellationToken.IsCancellationRequested && _queue.TryTake(out ChangeRecord record)) {
				Deliver(record, cancellationToken);
			}
		}

		internal void CloseStreams() {
			foreach (KeyValuePair<string, IChangeStream> stream in _streams) {
				try {
					stream.Value.Close();
				} catch (Exception exception) {
					_log.WarnFormat("Closing the Databricks stream to {0} failed: {1}", stream.Key, exception.Message);
				} finally {
					stream.Value.Dispose();
				}
			}
			_streams.Clear();
			_streamsOptionsKey = null;
		}

		private void Run(CancellationToken cancellationToken) {
			try {
				while (!cancellationToken.IsCancellationRequested) {
					ChangeRecord record;
					try {
						if (!_queue.TryTake(out record, Timeout.Infinite, cancellationToken)) {
							continue;
						}
					} catch (OperationCanceledException) {
						break;
					}
					Deliver(record, cancellationToken);
				}
			} catch (Exception exception) {
				_log.Error("Databricks publisher worker terminated unexpectedly.", exception);
			} finally {
				CloseStreams();
				lock (_syncRoot) {
					if (_worker == Thread.CurrentThread) {
						_worker = null;
						_cancellation?.Dispose();
						_cancellation = null;
					}
				}
			}
		}

		private void Deliver(ChangeRecord record, CancellationToken cancellationToken) {
			string json;
			try {
				json = record.ToJson();
			} catch (Exception exception) {
				WarnRateLimited("serialize", "Cannot serialize the change of {0} {1}; dropping it: {2}",
					record.EntitySchemaName, record.RecordId, exception.Message);
				return;
			}
			int attempt = 0;
			while (!cancellationToken.IsCancellationRequested) {
				DatabricksOptions options = ReadOptions();
				if (options == null || !options.IsConfigured) {
					WarnRateLimited("not-configured",
						"Databricks system settings are not configured; dropping change of {0} {1}.",
						record.EntitySchemaName, record.RecordId);
					return;
				}
				EnsureStreamsMatch(options);
				try {
					ErrorOr<IChangeStream> stream = GetStream(options, record.TargetTable);
					if (stream.IsError) {
						WarnRateLimited("stream:" + stream.FirstError.Code, "Cannot publish to Databricks ({0}): {1}",
							stream.FirstError.Code, stream.FirstError.Description);
						return;
					}
					stream.Value.Ingest(json);
					return;
				} catch (ZerobusException exception) {
					DropStream(record.TargetTable);
					WarnRateLimited("ingest:" + record.TargetTable,
						"Publishing to Databricks table {0} failed (retryable: {1}): {2}",
						record.TargetTable, exception.IsRetryable, exception.RawMessage);
					if (!exception.IsRetryable && attempt > 0) {
						return;
					}
				} catch (Exception exception) {
					// Not a Zerobus error: retrying cannot be assumed to help, so give up after a few attempts.
					DropStream(record.TargetTable);
					WarnRateLimited("ingest:" + record.TargetTable, "Publishing to Databricks table {0} failed: {1}",
						record.TargetTable, exception.Message);
					if (attempt + 1 >= Constants.Publisher.MaxUnexpectedErrorAttempts) {
						return;
					}
				}
				attempt++;
				if (!Delay(GetRetryDelay(attempt), cancellationToken)) {
					return;
				}
			}
		}

		private DatabricksOptions ReadOptions() {
			try {
				AppConnection appConnection = _appConnection;
				return _optionsProvider.Get(appConnection?.SystemUserConnection);
			} catch (Exception exception) {
				WarnRateLimited("options", "Cannot read Databricks system settings: {0}", exception.Message);
				return null;
			}
		}

		private void EnsureStreamsMatch(DatabricksOptions options) {
			string optionsKey = options.ConnectionKey + "|" + options.ClientSecret;
			if (_streamsOptionsKey != null && _streamsOptionsKey != optionsKey) {
				_log.Info("Databricks system settings changed; reopening streams.");
				CloseStreams();
			}
			_streamsOptionsKey = optionsKey;
		}

		private ErrorOr<IChangeStream> GetStream(DatabricksOptions options, string tableName) {
			if (_streams.TryGetValue(tableName, out IChangeStream existing)) {
				return ErrorOrFactory.From(existing);
			}
			ErrorOr<IChangeStream> created = _streamFactory.Create(options, tableName);
			if (!created.IsError) {
				_streams[tableName] = created.Value;
				_log.InfoFormat("Opened Databricks stream to {0} ({1}).", tableName, options);
			}
			return created;
		}

		private void DropStream(string tableName) {
			if (_streams.TryGetValue(tableName, out IChangeStream stream)) {
				_streams.Remove(tableName);
				try {
					stream.Dispose();
				} catch (Exception exception) {
					_log.WarnFormat("Disposing the Databricks stream to {0} failed: {1}", tableName, exception.Message);
				}
			}
		}

		private static TimeSpan GetRetryDelay(int attempt) {
			double seconds = Math.Min(Constants.Publisher.MaxRetryDelaySeconds, Math.Pow(2, attempt - 1));
			return TimeSpan.FromSeconds(seconds);
		}

		private void WarnRateLimited(string key, string format, params object[] args) {
			DateTime now = _utcNow();
			DateTime last = _lastWarnings.GetOrAdd(key, DateTime.MinValue);
			if (now - last < TimeSpan.FromSeconds(Constants.Publisher.WarningIntervalSeconds)) {
				return;
			}
			_lastWarnings[key] = now;
			_log.WarnFormat(format, args);
		}
	}
}
