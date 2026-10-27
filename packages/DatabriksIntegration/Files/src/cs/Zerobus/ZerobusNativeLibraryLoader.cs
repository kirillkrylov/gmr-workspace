using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Databricks.Zerobus;
using ErrorOr;

namespace DatabriksIntegrationApp.Zerobus {

	internal interface IZerobusNativeLibraryLoader {
		/// <summary>
		/// Provisions (first use only) and loads the native zerobus_ffi library; returns its path.
		/// </summary>
		ErrorOr<string> EnsureLoaded();
	}

	// Platform probing is verified by the build/deployment and the live run on the environment.
	[ExcludeFromCodeCoverage]
	internal sealed class ZerobusNativeLibraryLoader : IZerobusNativeLibraryLoader {
		private readonly object _syncRoot = new object();
		private readonly ZerobusNativeRuntimeProvisioner _provisioner = new ZerobusNativeRuntimeProvisioner();

		public ErrorOr<string> EnsureLoaded() {
			if (ZerobusNativeLibrary.IsLoaded) {
				return ZerobusNativeLibrary.LoadedPath;
			}
			lock (_syncRoot) {
				if (ZerobusNativeLibrary.IsLoaded) {
					return ZerobusNativeLibrary.LoadedPath;
				}
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
						RuntimeInformation.ProcessArchitecture != Architecture.X64) {
					return Error.Failure("Zerobus.NativeUnsupportedPlatform",
						"The package ships the Zerobus native library for Windows x64 only; this host is " +
						RuntimeInformation.OSDescription + " " + RuntimeInformation.ProcessArchitecture + ".");
				}
				try {
					string path = FindOrProvisionPath("win-x64", ZerobusNativeLibrary.PlatformFileName);
					ZerobusNativeLibrary.Load(path);
					return path;
				} catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException ||
						exception is InvalidDataException || exception is DllNotFoundException) {
					return Error.Failure("Zerobus.NativeLoadFailed", exception.Message);
				}
			}
		}

		private string FindOrProvisionPath(string runtime, string fileName) {
			foreach (string root in GetSearchRoots()) {
				if (!IsApplicationRoot(root)) {
					continue;
				}
				string versionedRuntimePath = Path.Combine(root, "conf", Constants.Zerobus.ExternalNativeDirectoryName,
					Constants.Zerobus.RuntimeProductName, Constants.Zerobus.NativeRuntimeVersion, runtime);
				string versionedLibraryPath = Path.Combine(versionedRuntimePath, "native", fileName);
				if (File.Exists(versionedLibraryPath)) {
					return versionedLibraryPath;
				}
				string artifactPath = FindArtifactPath(root);
				if (!string.IsNullOrWhiteSpace(artifactPath)) {
					return _provisioner.Provision(artifactPath, versionedRuntimePath, runtime, fileName);
				}
			}
			throw new FileNotFoundException("The packaged Zerobus native runtime could not be discovered. Expected " +
				"Files/" + Constants.Zerobus.NativeArtifactsDirectoryName + "/" +
				Constants.Zerobus.NativeRuntimeArtifactFileName + " in package " + Constants.PackageName + ".");
		}

		private static string FindArtifactPath(string applicationRoot) {
			string packageFilesPath = Path.Combine(applicationRoot, "Terrasoft.Configuration", "Pkg",
				Constants.PackageName, "Files");
			string artifactPath = Path.Combine(packageFilesPath, Constants.Zerobus.NativeArtifactsDirectoryName,
				Constants.Zerobus.NativeRuntimeArtifactFileName);
			return File.Exists(artifactPath) ? artifactPath : null;
		}

		private static bool IsApplicationRoot(string path) {
			return Directory.Exists(Path.Combine(path, "conf")) &&
				Directory.Exists(Path.Combine(path, "Terrasoft.Configuration"));
		}

		private static IEnumerable<string> GetSearchRoots() {
			var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			string assemblyLocation = typeof(ZerobusNativeLibraryLoader).GetTypeInfo().Assembly.Location;
			if (!string.IsNullOrWhiteSpace(assemblyLocation)) {
				AddWithParents(roots, Path.GetDirectoryName(assemblyLocation));
			}
			AddWithParents(roots, AppContext.BaseDirectory);
			AddWithParents(roots, Directory.GetCurrentDirectory());
			return roots;
		}

		private static void AddWithParents(ISet<string> roots, string path) {
			if (string.IsNullOrWhiteSpace(path)) {
				return;
			}
			var directory = new DirectoryInfo(path);
			for (int level = 0; directory != null && level < 10; level++, directory = directory.Parent) {
				roots.Add(directory.FullName);
			}
		}
	}
}
