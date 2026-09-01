using System;
using System.IO;
using Xunit;

namespace ComicRack.Tests.HiDpi
{
	public class ViewConfigRegressionGuardTests
	{
		[Fact]
		public void ComicBrowserControl_DoesNotDefineNormalizeViewConfigSizes()
		{
			AssertSourceDoesNotContain("ComicRack", "Views", "ComicBrowserControl.cs", "NormalizeViewConfigSizes");
		}

		[Fact]
		public void ComicBrowserControl_DoesNotRescaleWorkspaceThumbOnLoad()
		{
			AssertSourceDoesNotContain("ComicRack", "Views", "ComicBrowserControl.cs", "ApplyLogicalDisplaySizes");
			AssertSourceDoesNotContain("ComicRack", "Views", "ComicBrowserControl.cs", "ApplyDisplayItemSize");
		}

		private static void AssertSourceDoesNotContain(string project, string subfolder, string fileName, string token)
		{
			var repoRoot = FindRepoRoot();
			var path = Path.Combine(repoRoot, project, subfolder, fileName);
			Assert.True(File.Exists(path), "Expected source at " + path);
			var source = File.ReadAllText(path);
			Assert.DoesNotContain(token, source);
		}

		private static string FindRepoRoot()
		{
			var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while (dir != null)
			{
				if (File.Exists(Path.Combine(dir.FullName, "ComicRack.sln")))
				{
					return dir.FullName;
				}
				dir = dir.Parent;
			}
			throw new InvalidOperationException("Could not locate repo root (ComicRack.sln)");
		}
	}
}
