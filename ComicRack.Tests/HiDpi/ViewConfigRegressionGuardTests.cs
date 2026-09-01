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
			var repoRoot = FindRepoRoot();
			var path = Path.Combine(repoRoot, "ComicRack", "Views", "ComicBrowserControl.cs");
			Assert.True(File.Exists(path), "Expected ComicBrowserControl.cs at " + path);

			var source = File.ReadAllText(path);
			Assert.DoesNotContain("NormalizeViewConfigSizes", source);
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
