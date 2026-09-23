using System.IO;
using System.Linq;
using cYo.Projects.ComicRack.Plugins;
using cYo.Projects.ComicRack.Plugins.WebView;
using Xunit;

namespace ComicRack.Tests.Plugins
{
	public class JsonPluginInitializerTests
	{
		[Fact]
		public void KindWeb_EmitsPrimaryAndConfigCommands()
		{
			string dir = CreateWebPackage();
			try
			{
				var init = new JsonPluginInitializer();
				var commands = init.GetCommands(Path.Combine(dir, "plugin.json")).ToList();
				Assert.Equal(2, commands.Count);
				Assert.Contains(commands, c => c.Hook == PluginEngine.ScriptTypeBooks && c.Key == "WebKindSample");
				Assert.Contains(commands, c => c.Hook == PluginEngine.ScriptTypeConfig && c.Key == "WebKindSample");
				Assert.All(commands, c => Assert.IsType<WebPluginCommand>(c));
			}
			finally
			{
				TryDeleteDir(dir);
			}
		}

		[Fact]
		public void KindPython_EmitsNothingFromJsonInitializer()
		{
			string dir = Path.Combine(Path.GetTempPath(), "cr-web-" + Path.GetRandomFileName());
			Directory.CreateDirectory(dir);
			try
			{
				File.WriteAllText(Path.Combine(dir, "plugin.json"),
					"""{"id":"Py","version":"1.0.0","apiVersion":1,"kind":"python","ui":{"configure":"ui/dist/index.html"}}""");
				Directory.CreateDirectory(Path.Combine(dir, "ui", "dist"));
				File.WriteAllText(Path.Combine(dir, "ui", "dist", "index.html"), "<html></html>");
				Assert.Empty(new JsonPluginInitializer().GetCommands(Path.Combine(dir, "plugin.json")));
			}
			finally
			{
				TryDeleteDir(dir);
			}
		}

		[Fact]
		public void AttachConfigure_PythonWinsOverWeb()
		{
			var primary = new WebPluginCommand
			{
				Hook = PluginEngine.ScriptTypeBooks,
				Key = "X",
				Name = "X",
				Enabled = true
			};
			primary.Initialize(new NullEnv(), Path.GetTempPath());

			var webCfg = new WebPluginCommand
			{
				Hook = PluginEngine.ScriptTypeConfig,
				Key = "X",
				Name = "web",
				Enabled = true
			};
			var pyCfg = new PythonCommand
			{
				Hook = PluginEngine.ScriptTypeConfig,
				Key = "X",
				Name = "py",
				Method = "Configure",
				ScriptFile = "dummy.py",
				Enabled = true
			};

			// Simulate FR-004 attach order: web first, then python
			Command attached = null;
			void Attach(Command cfg)
			{
				if (attached == null)
				{
					attached = cfg;
				}
				else if (cfg is PythonCommand && attached is WebPluginCommand)
				{
					attached = cfg;
				}
			}
			Attach(webCfg);
			Attach(pyCfg);
			Assert.IsType<PythonCommand>(attached);
		}

		private static string CreateWebPackage()
		{
			string dir = Path.Combine(Path.GetTempPath(), "cr-web-" + Path.GetRandomFileName());
			Directory.CreateDirectory(Path.Combine(dir, "ui", "dist"));
			File.WriteAllText(Path.Combine(dir, "ui", "dist", "index.html"), "<html></html>");
			File.WriteAllText(Path.Combine(dir, "plugin.json"),
				"""{"id":"WebKindSample","version":"1.0.0","apiVersion":1,"kind":"web","ui":{"configure":"ui/dist/index.html"},"hooks":[{"type":"Books","key":"WebKindSample","name":"Web Kind Sample"}]}""");
			return dir;
		}

		private static void TryDeleteDir(string path)
		{
			try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
		}

		private sealed class NullEnv : IPluginEnvironment
		{
			public System.Windows.Forms.IWin32Window MainWindow => null;
			public cYo.Projects.ComicRack.Plugins.Automation.IApplication App => null;
			public cYo.Projects.ComicRack.Plugins.Automation.IOpenBooksManager OpenBooks => null;
			public cYo.Projects.ComicRack.Plugins.Automation.IBrowser Browser => null;
			public cYo.Projects.ComicRack.Engine.Display.IComicDisplay ComicDisplay => null;
			public string CommandPath { get; set; }
			public cYo.Projects.ComicRack.Plugins.Theme.IThemePlugin Theme { get; set; }
			public System.Collections.Generic.IEnumerable<string> LibraryPaths => System.Array.Empty<string>();
			public string Localize(string resourceKey, string elementKey, string text) => text;
			public object Clone() => this;
			public System.Windows.Forms.DialogResult ShowWebConfigure(string packageDirectory = null) => System.Windows.Forms.DialogResult.Cancel;
		}
	}
}
