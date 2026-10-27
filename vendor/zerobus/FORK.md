# Zerobus .NET SDK — netstandard2.0 fork

Source: [databricks/zerobus-sdk](https://github.com/databricks/zerobus-sdk) `dotnet/` at commit
`122572a3598d0172dadfe4d90680ef4602a7c141`, the source of NuGet `Databricks.Zerobus.Ingest.Sdk` 0.1.0.
Licensed under Apache-2.0 (`LICENSE`, `NOTICE` kept unchanged). Modified files are listed below.

Purpose: run the official SDK inside Creatio on both .NET 10 and .NET Framework 4.7.2. The Rust native
library (`zerobus_ffi`) is **not** rebuilt; the official prebuilt binaries from the 0.1.0 NuGet package are used.

## Changes

| File | Change |
|---|---|
| `src/Zerobus/Zerobus.csproj` | Replaced. Targets `netstandard2.0`, `LangVersion latest` (C# 14), no cargo build, no native packing. Adds `PolySharp`, `System.Memory`, `Microsoft.Bcl.AsyncInterfaces`. |
| `src/Zerobus/Native/NetStandardPolyfills.cs` | Added. `ArgumentNullException.ThrowIfNull`, `ObjectDisposedException.ThrowIf`, `Marshal.PtrToStringUTF8` (C# 14 static extensions), `Encoding.GetString(ReadOnlySpan<byte>)`, `Encoding.GetBytes(string, byte[])`, `KeyValuePair.Deconstruct`, non-generic `TaskCompletionSource`, `LPUTF8Str` constant. Upstream call sites compile unchanged. |
| `src/Zerobus/ZerobusNativeLibrary.cs` | Added. Public `Load(path)` preloads `zerobus_ffi` by absolute path (`LoadLibraryW` / `dlopen`) and, on .NET Core, registers a `DllImportResolver` via reflection. |
| `src/Zerobus/StreamConfigurationOptionsFactory.cs` | Added. `Create(...)` builds `StreamConfigurationOptions` for C# 7.3 callers, which cannot assign init-only properties. |
| `src/Zerobus/Native/NativeBindings.cs` | Modified. `UnmanagedType.LPUTF8Str` → `NetStandardPolyfills.LPUTF8Str` (14 sites); `NativeLibrary` resolver replaced by `ZerobusNativeLibrary.EnsureInitialized()`; `zerobus_get_default_config` returns a blittable mirror struct (.NET Framework throws `MarshalDirectiveException` on struct returns with marshalled `bool`s); `zerobus_stream_is_closed` gets `[return: MarshalAs(U1)]` (upstream reads a 1-byte C `bool` as a 4-byte `BOOL`). |
| `tests/Directory.Build.props`, `tests/Directory.Packages.props` | Added. Recreate the upstream root props the test projects rely on. |
| `tests/Directory.Build.targets` | Added. Copies `zerobus_ffi.dll` from the 0.1.0 NuGet package next to the tests; `-p:ZerobusTestNetFx=true` runs the unit tests on `net472` x64. |
| `tools/ZerobusSmoke/` | Added. Live smoke test against a real workspace, `net10.0` and `net472`. |

Upstream test sources are unchanged.

## Verification (2026-10-23)

- `tests/Zerobus.Tests`: 47/47 on `net10.0`, 47/47 on `net472` (x64).
- `tests/Zerobus.IntegrationTests` (local mock gRPC server, real native library): 54/54 on `net10.0`.
- `tools/ZerobusSmoke` against Databricks Free Edition (AWS us-west-2), table `workspace.default.creatio_changes`:
  one record acknowledged and queried back from each of `net10.0` and .NET Framework 4.8 x64.

```bash
dotnet test tests/Zerobus.Tests/Zerobus.Tests.csproj -f net10.0
dotnet test tests/Zerobus.Tests/Zerobus.Tests.csproj -p:ZerobusTestNetFx=true
dotnet test tests/Zerobus.IntegrationTests/Zerobus.IntegrationTests.csproj -f net10.0
dotnet run --project tools/ZerobusSmoke -f net10.0
```

## Updating to a new upstream release

Take the new `dotnet/src/Zerobus` from the commit the new NuGet was built from, re-apply the
`NativeBindings.cs` changes above, re-run the commands above, and bump the NuGet version used for the native binaries.
