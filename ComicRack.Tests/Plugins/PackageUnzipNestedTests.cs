using System.IO;
using System.Text;
using cYo.Projects.ComicRack.Engine;
using ICSharpCode.SharpZipLib.Zip;
using Xunit;

namespace ComicRack.Tests.Plugins
{
	public class PackageUnzipNestedTests
	{
		[Fact]
		public void UnzipFile_PreservesNestedRelativePaths()
		{
			string zipPath = Path.Combine(Path.GetTempPath(), "cr-nested-" + Path.GetRandomFileName() + ".zip");
			string target = Path.Combine(Path.GetTempPath(), "cr-out-" + Path.GetRandomFileName());
			try
			{
				CreateZip(zipPath, new[]
				{
					("ui/dist/index.html", "<html></html>"),
					("Package.ini", "Name=Test")
				});

				PackageManager.Package.UnzipFile(zipPath, target);

				Assert.True(File.Exists(Path.Combine(target, "ui", "dist", "index.html")));
				Assert.True(File.Exists(Path.Combine(target, "Package.ini")));
			}
			finally
			{
				TryDelete(zipPath);
				TryDeleteDir(target);
			}
		}

		[Fact]
		public void UnzipFile_RejectsZipSlip()
		{
			string zipPath = Path.Combine(Path.GetTempPath(), "cr-slip-" + Path.GetRandomFileName() + ".zip");
			string target = Path.Combine(Path.GetTempPath(), "cr-out-" + Path.GetRandomFileName());
			string escapeProbe = Path.Combine(Path.GetTempPath(), "cr-escape-" + Path.GetRandomFileName() + ".txt");
			try
			{
				Directory.CreateDirectory(target);
				CreateZip(zipPath, new[]
				{
					("../" + Path.GetFileName(escapeProbe), "pwned")
				});

				PackageManager.Package.UnzipFile(zipPath, target);

				Assert.False(File.Exists(escapeProbe));
				Assert.Empty(Directory.GetFiles(target, "*", SearchOption.AllDirectories));
			}
			finally
			{
				TryDelete(zipPath);
				TryDelete(escapeProbe);
				TryDeleteDir(target);
			}
		}

		[Fact]
		public void UnzipFile_FlatPackageStillWorks()
		{
			string zipPath = Path.Combine(Path.GetTempPath(), "cr-flat-" + Path.GetRandomFileName() + ".zip");
			string target = Path.Combine(Path.GetTempPath(), "cr-out-" + Path.GetRandomFileName());
			try
			{
				CreateZip(zipPath, new[] { ("hello.txt", "ok") });
				PackageManager.Package.UnzipFile(zipPath, target);
				Assert.True(File.Exists(Path.Combine(target, "hello.txt")));
			}
			finally
			{
				TryDelete(zipPath);
				TryDeleteDir(target);
			}
		}

		private static void CreateZip(string zipPath, (string name, string content)[] entries)
		{
			using (var zip = ZipFile.Create(zipPath))
			{
				zip.BeginUpdate();
				foreach (var (name, content) in entries)
				{
					var data = new MemoryDataSource(Encoding.UTF8.GetBytes(content));
					zip.Add(data, name);
				}
				zip.CommitUpdate();
			}
		}

		private sealed class MemoryDataSource : IStaticDataSource
		{
			private readonly byte[] bytes;

			public MemoryDataSource(byte[] bytes) => this.bytes = bytes;

			public Stream GetSource() => new MemoryStream(bytes);
		}

		private static void TryDelete(string path)
		{
			try { if (File.Exists(path)) File.Delete(path); } catch { }
		}

		private static void TryDeleteDir(string path)
		{
			try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
		}
	}
}
