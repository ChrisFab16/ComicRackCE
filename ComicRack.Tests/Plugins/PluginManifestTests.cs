using System.IO;
using cYo.Projects.ComicRack.Plugins.WebView;
using Xunit;

namespace ComicRack.Tests.Plugins
{
	public class PluginManifestTests
	{
		[Fact]
		public void TryLoad_ValidManifest()
		{
			string dir = Path.Combine(Path.GetTempPath(), "cr-manifest-" + Path.GetRandomFileName());
			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "plugin.json"),
					"""{"id":"demo","version":"1.0.0","apiVersion":1,"ui":{"configure":"ui/dist/index.html"}}""");
				Directory.CreateDirectory(Path.Combine(dir, "ui", "dist"));
				File.WriteAllText(Path.Combine(dir, "ui", "dist", "index.html"), "<html></html>");

				Assert.True(PluginManifest.TryLoad(dir, out PluginManifest m, out string error));
				Assert.Null(error);
				Assert.Equal("demo", m.Id);
				Assert.NotNull(m.ResolveConfigureEntry(dir));
			}
			finally
			{
				try { Directory.Delete(dir, true); } catch { }
			}
		}

		[Fact]
		public void TryLoad_RejectsFutureApiVersion()
		{
			string dir = Path.Combine(Path.GetTempPath(), "cr-manifest-" + Path.GetRandomFileName());
			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "plugin.json"),
					"""{"id":"demo","version":"1.0.0","apiVersion":99}""");
				Assert.False(PluginManifest.TryLoad(dir, out _, out string error));
				Assert.Contains("newer", error);
			}
			finally
			{
				try { Directory.Delete(dir, true); } catch { }
			}
		}
	}
}
