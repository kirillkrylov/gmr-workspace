using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace DatabriksIntegrationApp.Zerobus {

	/// <summary>
	/// Extracts one runtime of the packaged native artifact into an immutable, versioned directory outside
	/// the package, so package updates never replace a native library a worker process has loaded.
	/// Adapted from the clio Kafka / Google Pub/Sub reference examples; the marker is the SHA-256 of the
	/// native library itself, so re-zipping the same binary (new timestamps) does not invalidate it.
	/// </summary>
	internal sealed class ZerobusNativeRuntimeProvisioner {
		private const string ArtifactHashFileName = ".artifact.sha256";

		internal string Provision(string artifactPath, string destinationPath, string runtime,
				string libraryFileName) {
			if (!File.Exists(artifactPath)) {
				throw new FileNotFoundException("The Zerobus native runtime artifact was not found.", artifactPath);
			}
			string libraryPath = Path.Combine(destinationPath, "native", libraryFileName);
			string libraryHash = ComputeLibraryEntryHash(artifactPath, runtime, libraryFileName);
			if (IsCurrent(destinationPath, libraryPath, libraryHash)) {
				return libraryPath;
			}
			if (Directory.Exists(destinationPath)) {
				throw new InvalidDataException("The existing Zerobus native runtime does not match its packaged artifact. " +
					"Deploy a new runtime version instead of replacing files that may be loaded.");
			}

			Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
			string stagingPath = destinationPath + ".staging-" + Guid.NewGuid().ToString("N");
			try {
				ExtractRuntime(artifactPath, stagingPath, runtime);
				File.WriteAllText(Path.Combine(stagingPath, ArtifactHashFileName), libraryHash);
				try {
					Directory.Move(stagingPath, destinationPath);
				} catch (IOException) when (IsCurrent(destinationPath, libraryPath, libraryHash)) {
					// Another worker atomically published the same artifact first.
				}
				return libraryPath;
			} finally {
				if (Directory.Exists(stagingPath)) {
					Directory.Delete(stagingPath, true);
				}
			}
		}

		private static string ComputeLibraryEntryHash(string artifactPath, string runtime, string libraryFileName) {
			string entryName = runtime + "/native/" + libraryFileName;
			using (ZipArchive archive = ZipFile.OpenRead(artifactPath)) {
				ZipArchiveEntry entry = archive.Entries.FirstOrDefault(item =>
					string.Equals(item.FullName.Replace('\\', '/'), entryName, StringComparison.OrdinalIgnoreCase));
				if (entry == null) {
					throw new InvalidDataException("The Zerobus artifact does not contain " + entryName + ".");
				}
				using (Stream stream = entry.Open()) {
					return ComputeHash(stream);
				}
			}
		}

		private static void ExtractRuntime(string artifactPath, string stagingPath, string runtime) {
			string prefix = runtime + "/";
			Directory.CreateDirectory(stagingPath);
			string normalizedStagingPath = Path.GetFullPath(stagingPath) + Path.DirectorySeparatorChar;
			using (ZipArchive archive = ZipFile.OpenRead(artifactPath)) {
				foreach (ZipArchiveEntry entry in archive.Entries.Where(item =>
						item.FullName.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) {
					string relativePath = entry.FullName.Replace('\\', '/').Substring(prefix.Length)
						.Replace('/', Path.DirectorySeparatorChar);
					if (string.IsNullOrEmpty(relativePath)) {
						continue;
					}
					string targetPath = Path.GetFullPath(Path.Combine(stagingPath, relativePath));
					if (!targetPath.StartsWith(normalizedStagingPath, StringComparison.OrdinalIgnoreCase)) {
						throw new InvalidDataException("The Zerobus artifact contains an invalid entry path.");
					}
					if (string.IsNullOrEmpty(entry.Name)) {
						Directory.CreateDirectory(targetPath);
						continue;
					}
					Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
					entry.ExtractToFile(targetPath, true);
				}
			}
		}

		private static bool IsCurrent(string destinationPath, string libraryPath, string libraryHash) {
			if (!File.Exists(libraryPath)) {
				return false;
			}
			string hashPath = Path.Combine(destinationPath, ArtifactHashFileName);
			if (File.Exists(hashPath) &&
					string.Equals(File.ReadAllText(hashPath).Trim(), libraryHash, StringComparison.OrdinalIgnoreCase)) {
				return true;
			}
			// A marker written by an older scheme: trust the library bytes, not the marker.
			using (FileStream stream = File.OpenRead(libraryPath)) {
				return string.Equals(ComputeHash(stream), libraryHash, StringComparison.OrdinalIgnoreCase);
			}
		}

		private static string ComputeHash(Stream stream) {
			using (SHA256 sha256 = SHA256.Create()) {
				return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
			}
		}
	}
}
