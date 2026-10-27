using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using DatabriksIntegrationApp.Zerobus;
using FluentAssertions;
using NUnit.Framework;

namespace DatabriksIntegration.Tests.Zerobus {

	[TestFixture(Category = "UnitTests")]
	public class ZerobusNativeRuntimeProvisionerTests {
		private const string Runtime = "win-x64";
		private const string LibraryFileName = "zerobus_ffi.dll";
		private string _root;
		private string _destination;
		private ZerobusNativeRuntimeProvisioner _provisioner;

		[SetUp]
		public void SetUp() {
			_root = Path.Combine(Path.GetTempPath(), "zerobus-provisioner-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_root);
			_destination = Path.Combine(_root, "conf", "native", "zerobus", "0.1.0", Runtime);
			_provisioner = new ZerobusNativeRuntimeProvisioner();
		}

		[TearDown]
		public void TearDown() {
			if (Directory.Exists(_root)) {
				Directory.Delete(_root, true);
			}
		}

		private string CreateArtifact(string name, params string[] entries) {
			return CreateArtifactWithContent(name, "v1", entries);
		}

		private string CreateArtifactWithContent(string name, string contentVersion, params string[] entries) {
			string path = Path.Combine(_root, name);
			using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create)) {
				foreach (string entry in entries) {
					using (Stream stream = archive.CreateEntry(entry).Open()) {
						byte[] content = Encoding.UTF8.GetBytes(entry + ":" + contentVersion);
						stream.Write(content, 0, content.Length);
					}
				}
			}
			return path;
		}

		[Test]
		public void Provision_ExtractsOnlyRequestedRuntime() {
			string artifact = CreateArtifact("a.zip", "win-x64/native/zerobus_ffi.dll", "linux-x64/native/libzerobus_ffi.so");

			string libraryPath = _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);

			libraryPath.Should().Be(Path.Combine(_destination, "native", LibraryFileName));
			File.Exists(libraryPath).Should().BeTrue();
			File.Exists(Path.Combine(_destination, "native", "libzerobus_ffi.so")).Should().BeFalse();
			File.Exists(Path.Combine(_destination, ".artifact.sha256")).Should().BeTrue();
			Directory.GetDirectories(Path.GetDirectoryName(_destination), "*.staging-*").Should().BeEmpty();
		}

		[Test]
		public void Provision_SameArtifactTwice_ReusesExistingDirectory() {
			string artifact = CreateArtifact("a.zip", "win-x64/native/zerobus_ffi.dll");
			string first = _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);

			string second = _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);

			second.Should().Be(first);
		}

		[Test]
		public void Provision_DifferentLibraryForExistingVersion_Throws() {
			_provisioner.Provision(CreateArtifact("a.zip", "win-x64/native/zerobus_ffi.dll"), _destination, Runtime,
				LibraryFileName);
			string changed = CreateArtifactWithContent("b.zip", "v2", "win-x64/native/zerobus_ffi.dll");

			Action act = () => _provisioner.Provision(changed, _destination, Runtime, LibraryFileName);

			act.Should().Throw<InvalidDataException>();
		}

		[Test]
		public void Provision_RezippedSameLibrary_ReusesExistingDirectory() {
			string first = _provisioner.Provision(CreateArtifact("a.zip", "win-x64/native/zerobus_ffi.dll"),
				_destination, Runtime, LibraryFileName);
			string rezipped = CreateArtifact("b.zip", "win-x64/native/zerobus_ffi.dll", "win-x64/native/readme.txt");

			_provisioner.Provision(rezipped, _destination, Runtime, LibraryFileName).Should().Be(first);
		}

		[Test]
		public void Provision_StaleMarkerButSameLibrary_IsAccepted() {
			string artifact = CreateArtifact("a.zip", "win-x64/native/zerobus_ffi.dll");
			string libraryPath = _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);
			File.WriteAllText(Path.Combine(_destination, ".artifact.sha256"), "marker-from-older-scheme");

			_provisioner.Provision(artifact, _destination, Runtime, LibraryFileName).Should().Be(libraryPath);
		}

		[Test]
		public void Provision_EntryEscapingStaging_Throws() {
			string artifact = CreateArtifact("a.zip", "win-x64/../../evil.dll", "win-x64/native/zerobus_ffi.dll");

			Action act = () => _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);

			act.Should().Throw<InvalidDataException>();
			Directory.Exists(_destination).Should().BeFalse();
		}

		[Test]
		public void Provision_RuntimeMissingFromArtifact_Throws() {
			string artifact = CreateArtifact("a.zip", "linux-x64/native/libzerobus_ffi.so");

			Action act = () => _provisioner.Provision(artifact, _destination, Runtime, LibraryFileName);

			act.Should().Throw<InvalidDataException>();
			Directory.Exists(_destination).Should().BeFalse();
		}

		[Test]
		public void Provision_MissingArtifact_Throws() {
			Action act = () => _provisioner.Provision(Path.Combine(_root, "none.zip"), _destination, Runtime,
				LibraryFileName);

			act.Should().Throw<FileNotFoundException>();
		}
	}
}
