using System;
using System.Threading;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace BatchFileProcessingApp.RevenueFeed {

	/// <summary>
	/// <see cref="IRevenueFeedLock"/> over the platform distributed lock, so runs processed on different application
	/// nodes are serialized too. Falls back to a process-wide lock when no distributed lock is registered.
	/// </summary>
	internal sealed class RevenueFeedLock : IRevenueFeedLock {

		private const string Key = "BatchFileProcessing.RevenueFeed.ApplyAndTotals";
		private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
		private static readonly SemaphoreSlim LocalLock = new SemaphoreSlim(1, 1);

		public IDisposable Acquire() {
			IDistributedLock distributedLock = FindDistributedLock();
			if (distributedLock != null) {
				IDisposable handle = distributedLock.TryLock(Key, Timeout);
				if (handle == null) {
					throw new TimeoutException(
						$"Another revenue feed run held the lock for more than {Timeout.TotalMinutes} minutes.");
				}
				return handle;
			}
			if (!LocalLock.Wait(Timeout)) {
				throw new TimeoutException($"Another revenue feed run held the lock for more than {Timeout.TotalMinutes} minutes.");
			}
			return new Releaser();
		}

		private static IDistributedLock FindDistributedLock() {
			try {
				return ClassFactory.Get<IDistributedLock>();
			} catch (Exception) {
				return null;
			}
		}

		private sealed class Releaser : IDisposable {
			private int _released;

			public void Dispose() {
				if (Interlocked.Exchange(ref _released, 1) == 0) {
					LocalLock.Release();
				}
			}
		}
	}
}
