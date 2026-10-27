// Fork addition (not in upstream databricks/zerobus-sdk).
// StreamConfigurationOptions is a record with init-only setters; C# 7.3 callers (Creatio packages) can read
// but not assign them. This factory exposes the same options through a plain method call.

namespace Databricks.Zerobus;

/// <summary>
/// Creates <see cref="StreamConfigurationOptions"/> from callers that cannot use object initializers
/// or <c>with</c> expressions on init-only properties.
/// </summary>
public static class StreamConfigurationOptionsFactory
{
    /// <summary>
    /// Returns <see cref="StreamConfigurationOptions.Default"/> with the given values replaced.
    /// A <c>null</c> argument keeps the default value.
    /// </summary>
    public static StreamConfigurationOptions Create(
        ulong? maxInflightRequests = null,
        bool? recovery = null,
        ulong? recoveryTimeoutMs = null,
        ulong? recoveryBackoffMs = null,
        uint? recoveryRetries = null,
        ulong? serverLackOfAckTimeoutMs = null,
        ulong? flushTimeoutMs = null)
    {
        var defaults = StreamConfigurationOptions.Default;
        return defaults with
        {
            MaxInflightRequests = maxInflightRequests ?? defaults.MaxInflightRequests,
            Recovery = recovery ?? defaults.Recovery,
            RecoveryTimeoutMs = recoveryTimeoutMs ?? defaults.RecoveryTimeoutMs,
            RecoveryBackoffMs = recoveryBackoffMs ?? defaults.RecoveryBackoffMs,
            RecoveryRetries = recoveryRetries ?? defaults.RecoveryRetries,
            ServerLackOfAckTimeoutMs = serverLackOfAckTimeoutMs ?? defaults.ServerLackOfAckTimeoutMs,
            FlushTimeoutMs = flushTimeoutMs ?? defaults.FlushTimeoutMs,
        };
    }
}
