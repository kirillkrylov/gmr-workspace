// Fork addition (not in upstream databricks/zerobus-sdk).
// Replaces the upstream NativeLibrary-based resolver, which netstandard2.0 cannot reference.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Databricks.Zerobus;

/// <summary>
/// Loads the native <c>zerobus_ffi</c> library from an explicit path before the first SDK call.
/// </summary>
/// <remarks>
/// Hosts that cannot ship the library next to the application (for example Creatio packages, which
/// provision native code into a versioned folder) call <see cref="Load"/> once at startup. The P/Invoke
/// declarations then bind to the already loaded module: on Windows by module name, and on .NET Core 3+
/// additionally through a <c>DllImportResolver</c> registered via reflection.
/// A native library cannot be unloaded, so the first successful <see cref="Load"/> wins for the process.
/// </remarks>
public static class ZerobusNativeLibrary
{
    /// <summary>The library name used by the P/Invoke declarations.</summary>
    public const string LibraryName = "zerobus_ffi";

    private const int RtldNow = 2;
    private static readonly object SyncRoot = new();
    private static IntPtr _handle;
    private static string? _loadedPath;
    private static int _initialized;

    /// <summary>Gets a value indicating whether <see cref="Load"/> has loaded the library.</summary>
    public static bool IsLoaded => _handle != IntPtr.Zero;

    /// <summary>Gets the absolute path passed to the successful <see cref="Load"/> call.</summary>
    public static string? LoadedPath => _loadedPath;

    /// <summary>
    /// Gets the platform file name of the native library (<c>zerobus_ffi.dll</c>, <c>libzerobus_ffi.so</c>
    /// or <c>libzerobus_ffi.dylib</c>).
    /// </summary>
    public static string PlatformFileName
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "zerobus_ffi.dll";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "libzerobus_ffi.dylib";
            return "libzerobus_ffi.so";
        }
    }

    /// <summary>
    /// Loads the native library from <paramref name="path"/>. Repeated calls after a successful load are
    /// no-ops, whatever path they pass.
    /// </summary>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="DllNotFoundException">The operating system refused to load the file.</exception>
    public static void Load(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        EnsureInitialized();

        if (IsLoaded)
            return;

        lock (SyncRoot)
        {
            if (IsLoaded)
                return;

            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("The Zerobus native library was not found.", fullPath);

            var handle = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? LoadLibraryW(fullPath)
                : Dlopen(fullPath, RtldNow);

            if (handle == IntPtr.Zero)
            {
                var detail = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Win32 error " + Marshal.GetLastWin32Error()
                    : Marshal.PtrToStringAnsi(Dlerror()) ?? "dlopen failed";
                throw new DllNotFoundException($"The Zerobus native library could not be loaded from '{fullPath}': {detail}.");
            }

            _loadedPath = fullPath;
            _handle = handle;
        }
    }

    /// <summary>
    /// Registers the .NET Core DllImport resolver once. Safe to call repeatedly; a no-op on .NET Framework.
    /// </summary>
    internal static void EnsureInitialized()
    {
        if (Interlocked.Exchange(ref _initialized, 1) != 0)
            return;

        var nativeLibrary = Type.GetType("System.Runtime.InteropServices.NativeLibrary", throwOnError: false);
        var resolverType = Type.GetType("System.Runtime.InteropServices.DllImportResolver", throwOnError: false);
        if (nativeLibrary is null || resolverType is null)
            return;

        var setResolver = nativeLibrary.GetMethod(
            "SetDllImportResolver",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(Assembly), resolverType },
            modifiers: null);
        var callback = typeof(ZerobusNativeLibrary).GetMethod(
            nameof(Resolve),
            BindingFlags.NonPublic | BindingFlags.Static);
        if (setResolver is null || callback is null)
            return;

        try
        {
            setResolver.Invoke(null, new object[]
            {
                typeof(ZerobusNativeLibrary).Assembly,
                Delegate.CreateDelegate(resolverType, callback),
            });
        }
        catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException)
        {
            // A resolver is already registered for this assembly; default probing still applies.
        }
    }

    // Matches System.Runtime.InteropServices.DllImportResolver. Returning IntPtr.Zero falls back to
    // the runtime's default probing.
    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        => string.Equals(libraryName, LibraryName, StringComparison.Ordinal) ? _handle : IntPtr.Zero;

    [DllImport("kernel32", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryW(string path);

    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern IntPtr Dlopen(string path, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlerror")]
    private static extern IntPtr Dlerror();
}
