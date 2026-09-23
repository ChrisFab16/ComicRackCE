using System;
using System.Windows.Forms;
using cYo.Projects.ComicRack.Engine.Display;
using cYo.Projects.ComicRack.Plugins.Automation;
using cYo.Projects.ComicRack.Plugins.Theme;

namespace cYo.Projects.ComicRack.Plugins
{
	public interface IPluginEnvironment : IPluginConfig, ICloneable
	{
		IWin32Window MainWindow
		{
			get;
		}

		IApplication App
		{
			get;
		}

		IOpenBooksManager OpenBooks
		{
			get;
		}

		IBrowser Browser
		{
			get;
		}

		IComicDisplay ComicDisplay
		{
			get;
		}

		string CommandPath
		{
			get;
			set;
		}

		IThemePlugin Theme { get; set; }

		string Localize(string resourceKey, string elementKey, string text);

		/// <summary>
		/// Open the package WebView2 Configure SPA declared in plugin.json (ui.configure).
		/// </summary>
		System.Windows.Forms.DialogResult ShowWebConfigure(string packageDirectory = null);
	}
}
