// P/Invoke bindings to the Rust FFI layer (zerobus-ffi).
// This is the .NET equivalent of ffi.go in the Go SDK.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Databricks.Zerobus.Native;

/// <summary>
/// A single header key-value pair for C FFI.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CHeader
{
    public IntPtr Key;   // char*
    public IntPtr Value; // char*
}

/// <summary>
/// A collection of headers returned from a managed callback.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CHeaders
{
    public IntPtr Headers;      // CHeader*
    public nuint Count;
    public IntPtr ErrorMessage; // char*
}

/// <summary>
/// Opaque SDK handle. We only ever hold pointers to this.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CZerobusSdk
{
    // Opaque - zero-sized in C, only used via pointer.
}

/// <summary>
/// Result struct returned by most FFI calls.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CResult
{
    [MarshalAs(UnmanagedType.U1)]
    public bool Success;

    public IntPtr ErrorMessage; // char* — must be freed with zerobus_free_error_message

    [MarshalAs(UnmanagedType.U1)]
    public bool IsRetryable;
}

/// <summary>
/// Opaque stream handle.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CZerobusStream
{
    // Opaque.
}

/// <summary>
/// Stream configuration options passed to the native layer.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CStreamConfigurationOptions
{
    public nuint MaxInflightRequests;

    [MarshalAs(UnmanagedType.U1)]
    public bool Recovery;

    public ulong RecoveryTimeoutMs;
    public ulong RecoveryBackoffMs;
    public uint RecoveryRetries;
    public ulong ServerLackOfAckTimeoutMs;
    public ulong FlushTimeoutMs;
    public int RecordType;
    public ulong StreamPausedMaxWaitTimeMs;

    [MarshalAs(UnmanagedType.U1)]
    public bool HasStreamPausedMaxWaitTimeMs;

    public ulong CallbackMaxWaitTimeMs;

    [MarshalAs(UnmanagedType.U1)]
    public bool HasCallbackMaxWaitTimeMs;

    // Optional ack callback function pointers and user data.
    // Keep null for .NET for now; this preserves ABI layout with Rust FFI.
    public IntPtr AckOnAck;
    public IntPtr AckOnError;
    public IntPtr AckUserData;
}

/// <summary>
/// Fork addition: blittable mirror of <see cref="CStreamConfigurationOptions"/> (bool fields as bytes)
/// so it can be a P/Invoke return type on .NET Framework.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CStreamConfigurationOptionsBlittable
{
    public nuint MaxInflightRequests;
    public byte Recovery;
    public ulong RecoveryTimeoutMs;
    public ulong RecoveryBackoffMs;
    public uint RecoveryRetries;
    public ulong ServerLackOfAckTimeoutMs;
    public ulong FlushTimeoutMs;
    public int RecordType;
    public ulong StreamPausedMaxWaitTimeMs;
    public byte HasStreamPausedMaxWaitTimeMs;
    public ulong CallbackMaxWaitTimeMs;
    public byte HasCallbackMaxWaitTimeMs;
    public IntPtr AckOnAck;
    public IntPtr AckOnError;
    public IntPtr AckUserData;

    public readonly CStreamConfigurationOptions ToManaged() => new()
    {
        MaxInflightRequests = MaxInflightRequests,
        Recovery = Recovery != 0,
        RecoveryTimeoutMs = RecoveryTimeoutMs,
        RecoveryBackoffMs = RecoveryBackoffMs,
        RecoveryRetries = RecoveryRetries,
        ServerLackOfAckTimeoutMs = ServerLackOfAckTimeoutMs,
        FlushTimeoutMs = FlushTimeoutMs,
        RecordType = RecordType,
        StreamPausedMaxWaitTimeMs = StreamPausedMaxWaitTimeMs,
        HasStreamPausedMaxWaitTimeMs = HasStreamPausedMaxWaitTimeMs != 0,
        CallbackMaxWaitTimeMs = CallbackMaxWaitTimeMs,
        HasCallbackMaxWaitTimeMs = HasCallbackMaxWaitTimeMs != 0,
        AckOnAck = AckOnAck,
        AckOnError = AckOnError,
        AckUserData = AckUserData,
    };
}

/// <summary>
/// Represents a single record (either Proto or JSON) returned by get_unacked_records.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CRecord
{
    [MarshalAs(UnmanagedType.U1)]
    public bool IsJson;

    public IntPtr Data;    // uint8_t*
    public nuint DataLen;
}

/// <summary>
/// An array of records returned by get_unacked_records.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct CRecordArray
{
    public IntPtr Records; // CRecord*
    public nuint Len;
}

/// <summary>
/// Callback signature for the headers provider.
/// Matches: CHeaders (*HeadersProviderCallback)(void* user_data)
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate CHeaders HeadersProviderCallback(IntPtr userData);

/// <summary>
/// Callback that releases the headers provider's user_data.
/// Matches: void (*HeadersProviderFreeCallback)(void* user_data)
/// </summary>
/// <remarks>
/// The FFI owns user_data and invokes this exactly once, after any in-flight
/// GetHeaders callback has returned — so it is safe to free the provider here.
/// May run on an internal SDK worker thread, so it must not throw across the
/// native boundary.
/// </remarks>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void HeadersProviderFreeCallback(IntPtr userData);

/// <summary>
/// Callback for async stream creation completion.
/// Matches: void (*CreateStreamAsyncCallback)(CZerobusStream* stream, const CResult* result, void* user_data)
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void CreateStreamAsyncCallback(IntPtr stream, IntPtr result, IntPtr userData);

/// <summary>
/// Callback for async offset-returning operations.
/// Matches: void (*OffsetAsyncCallback)(int64_t offset, const CResult* result, void* user_data)
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void OffsetAsyncCallback(long offset, IntPtr result, IntPtr userData);

/// <summary>
/// Callback for async bool-returning operations.
/// Matches: void (*BoolAsyncCallback)(bool value, const CResult* result, void* user_data)
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void BoolAsyncCallback([MarshalAs(UnmanagedType.U1)] bool value, IntPtr result, IntPtr userData);

/// <summary>
/// Callback for async record-array-returning operations.
/// Matches: void (*RecordArrayAsyncCallback)(CRecordArray records, const CResult* result, void* user_data)
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void RecordArrayAsyncCallback(CRecordArray records, IntPtr result, IntPtr userData);

/// <summary>
/// P/Invoke declarations for the zerobus_ffi native library.
/// </summary>
internal static partial class NativeMethods
{
    private const string LibName = "zerobus_ffi";

    // Fork change: the upstream static constructor registered a NativeLibrary.SetDllImportResolver
    // (.NET Core 3+ only) probing AppContext.BaseDirectory/runtimes/<rid>/native. On netstandard2.0 the
    // host preloads the library by absolute path through ZerobusNativeLibrary.Load; without a preload the
    // runtime's default DllImport probing applies (e.g. zerobus_ffi.dll next to the application).
    static NativeMethods() => ZerobusNativeLibrary.EnsureInitialized();

    // --- SDK lifecycle ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_free")]
    public static extern void SdkFree(IntPtr sdk);

    // --- Stream creation ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_create_stream")]
    public static extern unsafe IntPtr SdkCreateStream(
        IntPtr sdk,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string tableName,
        byte* descriptorProtoBytes,
        nuint descriptorProtoLen,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string clientId,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string clientSecret,
        ref CStreamConfigurationOptions options,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_create_stream_with_headers_provider")]
    public static extern unsafe IntPtr SdkCreateStreamWithHeadersProvider(
        IntPtr sdk,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string tableName,
        byte* descriptorProtoBytes,
        nuint descriptorProtoLen,
        HeadersProviderCallback headersCallback,
        IntPtr userData,
        HeadersProviderFreeCallback? freeUserData,
        ref CStreamConfigurationOptions options,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_create_stream_with_headers_provider_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern unsafe bool SdkCreateStreamWithHeadersProviderAsync(
        IntPtr sdk,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string tableName,
        byte* descriptorProtoBytes,
        nuint descriptorProtoLen,
        HeadersProviderCallback headersCallback,
        IntPtr userData,
        HeadersProviderFreeCallback? freeUserData,
        ref CStreamConfigurationOptions options,
        CreateStreamAsyncCallback callback,
        IntPtr callbackUserData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_create_stream_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern unsafe bool SdkCreateStreamAsync(
        IntPtr sdk,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string tableName,
        byte* descriptorProtoBytes,
        nuint descriptorProtoLen,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string clientId,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string clientSecret,
        ref CStreamConfigurationOptions options,
        CreateStreamAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_recreate_stream")]
    public static extern IntPtr SdkRecreateStream(
        IntPtr sdk,
        IntPtr stream,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_recreate_stream_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool SdkRecreateStreamAsync(
        IntPtr sdk,
        IntPtr stream,
        CreateStreamAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    // Fork change: C `bool` is one byte; without U1 the default 4-byte BOOL marshalling reads undefined
    // upper bytes of the return register.
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_is_closed")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamIsClosed(IntPtr stream);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_free")]
    public static extern void StreamFree(IntPtr stream);

    // --- Record ingestion ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_proto_record")]
    public static extern unsafe long StreamIngestProtoRecord(
        IntPtr stream,
        byte* data,
        nuint dataLen,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_proto_record_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern unsafe bool StreamIngestProtoRecordAsync(
        IntPtr stream,
        byte* data,
        nuint dataLen,
        OffsetAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_json_record")]
    public static extern long StreamIngestJsonRecord(
        IntPtr stream,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string jsonData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_json_record_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamIngestJsonRecordAsync(
        IntPtr stream,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string jsonData,
        OffsetAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_proto_records")]
    public static extern unsafe long StreamIngestProtoRecords(
        IntPtr stream,
        byte** records,
        nuint* recordLens,
        nuint numRecords,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_proto_records_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern unsafe bool StreamIngestProtoRecordsAsync(
        IntPtr stream,
        byte** records,
        nuint* recordLens,
        nuint numRecords,
        OffsetAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_json_records")]
    public static extern unsafe long StreamIngestJsonRecords(
        IntPtr stream,
        byte** jsonRecords,
        nuint numRecords,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_ingest_json_records_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern unsafe bool StreamIngestJsonRecordsAsync(
        IntPtr stream,
        byte** jsonRecords,
        nuint numRecords,
        OffsetAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    // --- Acknowledgment / flush ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_wait_for_offset")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamWaitForOffset(
        IntPtr stream,
        long offset,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_wait_for_offset_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamWaitForOffsetAsync(
        IntPtr stream,
        long offset,
        BoolAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_flush")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamFlush(IntPtr stream, ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_flush_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamFlushAsync(
        IntPtr stream,
        BoolAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_get_unacked_records")]
    public static extern CRecordArray StreamGetUnackedRecords(IntPtr stream, ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_get_unacked_records_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamGetUnackedRecordsAsync(
        IntPtr stream,
        RecordArrayAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_free_record_array")]
    public static extern void FreeRecordArray(CRecordArray array);

    // --- Stream close ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_close")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamClose(IntPtr stream, ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_stream_close_async")]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool StreamCloseAsync(
        IntPtr stream,
        BoolAsyncCallback callback,
        IntPtr userData,
        ref CResult result);

    // --- Memory management ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_free_error_message")]
    public static extern void FreeErrorMessage(IntPtr message);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_alloc_header_array")]
    public static extern IntPtr AllocHeaderArray(nuint count);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_alloc_cstring")]
    public static extern unsafe IntPtr AllocCString(byte* data, nuint len);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_free_headers")]
    public static extern void FreeHeaders(CHeaders headers);

    // --- Configuration ---

    // Fork change: .NET Framework rejects P/Invoke return types that need marshalling (the bool fields of
    // CStreamConfigurationOptions) with MarshalDirectiveException, so the native call returns a blittable
    // mirror with the same layout and converts it.
    public static CStreamConfigurationOptions GetDefaultConfig() => GetDefaultConfigBlittable().ToManaged();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_get_default_config")]
    private static extern CStreamConfigurationOptionsBlittable GetDefaultConfigBlittable();

    // --- SDK builder ---

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_new")]
    public static extern IntPtr SdkBuilderNew();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_endpoint")]
    public static extern void SdkBuilderEndpoint(
        IntPtr builder,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_unity_catalog_url")]
    public static extern void SdkBuilderUnityCatalogUrl(
        IntPtr builder,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_sdk_identifier")]
    public static extern void SdkBuilderSdkIdentifier(
        IntPtr builder,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_application_name")]
    public static extern void SdkBuilderApplicationName(
        IntPtr builder,
        [MarshalAs(NetStandardPolyfills.LPUTF8Str)] string value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_disable_tls")]
    public static extern void SdkBuilderDisableTls(IntPtr builder);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_build")]
    public static extern IntPtr SdkBuilderBuild(IntPtr builder, ref CResult result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "zerobus_sdk_builder_free")]
    public static extern void SdkBuilderFree(IntPtr builder);
}
