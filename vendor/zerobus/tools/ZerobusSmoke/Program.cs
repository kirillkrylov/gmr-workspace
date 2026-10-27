using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Databricks.Zerobus;

// Usage: ZerobusSmoke [configPath] [tableName]
// configPath defaults to %USERPROFILE%\.zerobus\gmr.json (workspaceUrl, zerobusEndpoint, clientId, clientSecret).

var configPath = args.Length > 0
    ? args[0]
    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zerobus", "gmr.json");
var tableName = args.Length > 1 ? args[1] : "workspace.default.creatio_changes";

using var config = JsonDocument.Parse(File.ReadAllText(configPath));
string Setting(string name) => config.RootElement.GetProperty(name).GetString()
    ?? throw new InvalidOperationException($"Missing '{name}' in {configPath}.");

Console.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription} ({RuntimeInformation.ProcessArchitecture})");

var nativePath = Path.Combine(AppContext.BaseDirectory, "native", "win-x64", ZerobusNativeLibrary.PlatformFileName);
ZerobusNativeLibrary.Load(nativePath);
Console.WriteLine($"Native library loaded from {ZerobusNativeLibrary.LoadedPath}");

var runtimeTag = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal)
    ? "net472"
    : "net10";
var record = JsonSerializer.Serialize(new Dictionary<string, object>
{
    ["entity"] = "ZerobusSmoke",
    ["record_id"] = Guid.NewGuid().ToString(),
    ["operation"] = "smoke-" + runtimeTag,
    ["changed_on"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000,
    ["payload"] = "{\"machine\":\"" + Environment.MachineName + "\"}",
});

var watch = Stopwatch.StartNew();
try
{
    using var sdk = ZerobusSdk.CreateBuilder()
        .Endpoint(Setting("zerobusEndpoint"))
        .UnityCatalogUrl(Setting("workspaceUrl"))
        .ApplicationName("creatio-gmr-smoke")
        .Build();

    using var stream = sdk.CreateJsonStream(tableName, Setting("clientId"), Setting("clientSecret"));
    Console.WriteLine($"Stream opened to {tableName} in {watch.ElapsedMilliseconds} ms");

    var offset = stream.IngestRecord(record);
    stream.WaitForOffset(offset);
    stream.Flush();
    stream.Close();
    Console.WriteLine($"OK: record acknowledged at offset {offset} in {watch.ElapsedMilliseconds} ms");
    Console.WriteLine(record);
    return 0;
}
catch (ZerobusException ex)
{
    Console.WriteLine($"Zerobus error after {watch.ElapsedMilliseconds} ms (retryable={ex.IsRetryable}): {ex.RawMessage}");
    return 2;
}
