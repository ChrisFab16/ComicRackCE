using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using cYo.Projects.ComicRack.Engine;
using cYo.Projects.ComicRack.Engine.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace cYo.Projects.ComicRack.Plugins.Controls
{
	public partial class HtmlComicPageControl : ComicPageControl
	{
		private string lastResult;
		private string originalConfig;
		private bool webViewReady;
		private bool webViewInitStarted;
		private string pendingResult;
		private IPluginEnvironment environment;

		public Func<ComicBook[], string> InfoFunction { get; set; }

		public Action<string> SaveConfigFunction { get; set; }

		public HtmlPanelScriptProvider Script { get; private set; }

		public object ScriptEngine
		{
			get => environment;
			set
			{
				environment = value as IPluginEnvironment;
				Script.BindEnvironment(environment);
			}
		}

		public string ScriptConfig
		{
			get => Script.Config;
			set => originalConfig = Script.Config = value;
		}

		public HtmlComicPageControl()
		{
			Script = new HtmlPanelScriptProvider();
			InitializeComponent();
			HandleCreated += async (s, e) => await EnsureWebViewAsync();
		}

		private async Task EnsureWebViewAsync()
		{
			if (webViewReady || webViewInitStarted || IsDisposed)
			{
				return;
			}
			webViewInitStarted = true;
			try
			{
				await webView.EnsureCoreWebView2Async(null);
				webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
				webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = EngineConfiguration.Default.HtmlInfoContextMenu;
				webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
				webView.CoreWebView2.AddHostObjectToScript("external", Script);
				await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
					"window.external = chrome.webview.hostObjects.sync.external;");
				webViewReady = true;
				if (pendingResult != null)
				{
					ApplyContent(pendingResult);
					pendingResult = null;
				}
			}
			catch (Exception ex)
			{
				webViewInitStarted = false;
				if (!IsDisposed)
				{
					MessageBox.Show(FindForm(),
						"WebView2 Runtime is required for HTML plugin panels.\n\n" + ex.Message,
						"WebView2 unavailable",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning);
				}
			}
		}

		protected override void OnShowInfo(IEnumerable<ComicBook> books)
		{
			base.OnShowInfo(books);
			if (InfoFunction == null)
			{
				return;
			}
			try
			{
				string text = InfoFunction(books.ToArray());
				if (text == lastResult)
				{
					return;
				}
				lastResult = text;
				if (!webViewReady)
				{
					pendingResult = text;
					_ = EnsureWebViewAsync();
					return;
				}
				ApplyContent(text);
			}
			catch
			{
				lastResult = null;
				if (webViewReady)
				{
					webView.NavigateToString(string.Empty);
				}
			}
		}

		private void ApplyContent(string text)
		{
			if (text == null)
			{
				webView.NavigateToString(string.Empty);
				return;
			}
			if (HtmlInfoContent.IsUrlResult(text, out string url))
			{
				webView.CoreWebView2.Navigate(url);
			}
			else
			{
				webView.NavigateToString(text);
			}
		}
	}
}
