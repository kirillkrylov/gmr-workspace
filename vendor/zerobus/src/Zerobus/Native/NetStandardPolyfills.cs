// Fork addition (not in upstream databricks/zerobus-sdk).
// netstandard2.0 lacks several .NET 5+ APIs the upstream SDK calls. These polyfills supply them with
// the same names so the upstream call sites compile unchanged. Static extension members need C# 14.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

// Declared in the root namespace so the polyfills resolve from every upstream file.
namespace Databricks.Zerobus;

internal static class NetStandardPolyfills
{
    /// <summary>
    /// <c>UnmanagedType.LPUTF8Str</c> (48). Missing from the netstandard2.0 reference surface but
    /// honoured by the runtime on .NET Framework 4.7+ and .NET Core.
    /// </summary>
    internal const UnmanagedType LPUTF8Str = (UnmanagedType)48;

    extension(ArgumentNullException)
    {
        public static void ThrowIfNull(
            [System.Diagnostics.CodeAnalysis.NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
                throw new ArgumentNullException(paramName);
        }
    }

    extension(ObjectDisposedException)
    {
        public static void ThrowIf(
            [System.Diagnostics.CodeAnalysis.DoesNotReturnIf(true)] bool condition,
            object instance)
        {
            if (condition)
                throw new ObjectDisposedException(instance?.GetType().FullName);
        }
    }

    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            return string.Empty;

        fixed (byte* ptr = bytes)
        {
            return encoding.GetString(ptr, bytes.Length);
        }
    }

    public static int GetBytes(this Encoding encoding, string chars, byte[] bytes)
        => encoding.GetBytes(chars, 0, chars.Length, bytes, 0);

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    extension(Marshal)
    {
        public static unsafe string? PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            var bytes = (byte*)ptr;
            var length = 0;
            while (bytes[length] != 0)
                length++;

            return Encoding.UTF8.GetString(bytes, length);
        }
    }
}

/// <summary>
/// Non-generic <c>TaskCompletionSource</c> (.NET 5+) built on <see cref="TaskCompletionSource{TResult}"/>.
/// </summary>
internal sealed class TaskCompletionSource
{
    private readonly TaskCompletionSource<bool> _inner;

    public TaskCompletionSource(TaskCreationOptions creationOptions)
    {
        _inner = new TaskCompletionSource<bool>(creationOptions);
    }

    public Task Task => _inner.Task;

    public bool TrySetResult() => _inner.TrySetResult(true);

    public bool TrySetException(Exception exception) => _inner.TrySetException(exception);

    public bool TrySetCanceled() => _inner.TrySetCanceled();

    public bool TrySetCanceled(CancellationToken cancellationToken) => _inner.TrySetCanceled(cancellationToken);
}
