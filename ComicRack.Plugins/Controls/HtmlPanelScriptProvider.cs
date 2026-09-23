using System;
using System.Runtime.InteropServices;
using cYo.Projects.ComicRack.Plugins.Automation;

namespace cYo.Projects.ComicRack.Plugins.Controls
{
	/// <summary>
	/// COM-visible facade for WebView2 host objects so panel HTML can keep using
	/// <c>window.external.ComicRack.OpenBooks.OpenFile</c> and <c>window.external.Config</c>.
	/// </summary>
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class HtmlPanelScriptProvider
	{
		public HtmlPanelComicRackProxy ComicRack { get; private set; }

		public string Config { get; set; }

		public void BindEnvironment(IPluginEnvironment env)
		{
			ComicRack = env == null ? null : new HtmlPanelComicRackProxy(env);
		}
	}

	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class HtmlPanelComicRackProxy
	{
		private readonly IPluginEnvironment env;

		public HtmlPanelComicRackProxy(IPluginEnvironment env)
		{
			this.env = env ?? throw new ArgumentNullException(nameof(env));
		}

		public HtmlPanelOpenBooksProxy OpenBooks => new HtmlPanelOpenBooksProxy(env.OpenBooks);

		public string ProductVersion => env.App?.ProductVersion ?? string.Empty;
	}

	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class HtmlPanelOpenBooksProxy
	{
		private readonly IOpenBooksManager openBooks;

		public HtmlPanelOpenBooksProxy(IOpenBooksManager openBooks)
		{
			this.openBooks = openBooks;
		}

		public bool OpenFile(string file, bool inNewSlot, int page)
		{
			return openBooks != null && openBooks.OpenFile(file, inNewSlot, page);
		}
	}
}
