using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace cYo.Projects.ComicRack.Plugins.WebView
{
	public sealed class WebViewPluginForm : Form
	{
		public const string VirtualHostName = "comicrack.plugin";

		private readonly string packageDirectory;
		private readonly string configureHtmlPath;
		private readonly bool hotReload;
		private readonly HostJsonRpcContext rpcContext;
		private WebView2 webView;
		private Panel loadingPanel;
		private Label loadingLabel;
		private ProgressBar loadingBar;
		private FileSystemWatcher watcher;
		private System.Windows.Forms.Timer debounceTimer;
		private bool closing;

		public WebViewPluginForm(string packageDirectory, string configureHtmlPath, PluginManifest manifest, HostJsonRpcContext rpcContext)
		{
			this.packageDirectory = packageDirectory ?? throw new ArgumentNullException(nameof(packageDirectory));
			this.configureHtmlPath = configureHtmlPath ?? throw new ArgumentNullException(nameof(configureHtmlPath));
			this.rpcContext = rpcContext ?? throw new ArgumentNullException(nameof(rpcContext));
			hotReload = manifest?.HotReloadEnabled ?? true;

			Text = string.IsNullOrEmpty(manifest?.Id) ? "Plugin Configure" : manifest.Id + " — Configure";
			Width = 900;
			Height = 700;
			StartPosition = FormStartPosition.CenterParent;
			MinimizeBox = false;
			ShowInTaskbar = false;

			webView = new WebView2
			{
				Dock = DockStyle.Fill
			};
			Controls.Add(webView);

			loadingPanel = new Panel
			{
				Dock = DockStyle.Fill,
				BackColor = SystemColors.Window
			};
			loadingLabel = new Label
			{
				Text = "Loading Configure…",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 11f, FontStyle.Regular)
			};
			loadingBar = new ProgressBar
			{
				Style = ProgressBarStyle.Marquee,
				MarqueeAnimationSpeed = 30,
				Dock = DockStyle.Bottom,
				Height = 24
			};
			loadingPanel.Controls.Add(loadingLabel);
			loadingPanel.Controls.Add(loadingBar);
			Controls.Add(loadingPanel);
			loadingPanel.BringToFront();

			rpcContext.ReloadUi = () => BeginInvoke(new Action(ReloadSafe));
			rpcContext.CloseUi = result => BeginInvoke(new Action(() =>
			{
				DialogResult = string.Equals(result, "cancel", StringComparison.OrdinalIgnoreCase)
					? DialogResult.Cancel
					: DialogResult.OK;
				Close();
			}));

			Load += async (s, e) => await InitializeAsync();
			FormClosed += (s, e) => DisposeWatcher();
		}

		private void HideLoadingOverlay()
		{
			if (loadingPanel == null || loadingPanel.IsDisposed)
			{
				return;
			}
			loadingPanel.Visible = false;
			loadingPanel.SendToBack();
		}

		private async Task InitializeAsync()
		{
			try
			{
				// Dedicated profile folder — sharing the process default with HtmlComicPageControl
				// (Info panel) can hang EnsureCoreWebView2Async / CreateAsync indefinitely.
				string userData = GetWebView2UserDataFolder("PluginConfigure");
				Directory.CreateDirectory(userData);
				loadingLabel.Text = "Starting WebView2…";
				CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, userData);
				await webView.EnsureCoreWebView2Async(env);
			}
			catch (Exception ex)
			{
				HideLoadingOverlay();
				MessageBox.Show(this,
					"Microsoft Edge WebView2 Runtime is required for plugin SPA UI.\n\n" + ex.Message,
					"WebView2 unavailable",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
				DialogResult = DialogResult.Cancel;
				Close();
				return;
			}

			webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
			webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
			webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
			webView.CoreWebView2.NavigationCompleted += (s, e) =>
			{
				if (!IsDisposed)
				{
					BeginInvoke(new Action(HideLoadingOverlay));
				}
			};

			string folder = Path.GetDirectoryName(configureHtmlPath);
			webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
				VirtualHostName,
				folder,
				CoreWebView2HostResourceAccessKind.Allow);

			loadingLabel.Text = "Loading Configure UI…";
			string fileName = Path.GetFileName(configureHtmlPath);
			webView.CoreWebView2.Navigate($"https://{VirtualHostName}/{fileName}");

			if (hotReload)
			{
				StartWatcher(folder);
			}
		}

		internal static string GetWebView2UserDataFolder(string purpose)
		{
			string root = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"ComicRack Community Edition",
				"WebView2",
				purpose ?? "Default");
			return root;
		}

		private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
		{
			string message;
			try
			{
				message = e.TryGetWebMessageAsString() ?? e.WebMessageAsJson;
			}
			catch
			{
				message = e.WebMessageAsJson;
			}
			string response = HostJsonRpc.HandleMessage(message, rpcContext);
			webView.CoreWebView2.PostWebMessageAsJson(response);
		}

		private void StartWatcher(string folder)
		{
			if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
			{
				return;
			}
			debounceTimer = new System.Windows.Forms.Timer { Interval = 300 };
			debounceTimer.Tick += (s, e) =>
			{
				debounceTimer.Stop();
				ReloadSafe();
			};
			watcher = new FileSystemWatcher(folder)
			{
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
			};
			FileSystemEventHandler handler = (s, e) =>
			{
				if (closing || IsDisposed)
				{
					return;
				}
				try
				{
					BeginInvoke(new Action(() =>
					{
						debounceTimer.Stop();
						debounceTimer.Start();
					}));
				}
				catch (ObjectDisposedException)
				{
				}
			};
			watcher.Changed += handler;
			watcher.Created += handler;
			watcher.Renamed += (s, e) => handler(s, e);
			watcher.EnableRaisingEvents = true;
		}

		private void ReloadSafe()
		{
			if (closing || IsDisposed || webView?.CoreWebView2 == null)
			{
				return;
			}
			webView.CoreWebView2.Reload();
		}

		private void DisposeWatcher()
		{
			closing = true;
			if (watcher != null)
			{
				watcher.EnableRaisingEvents = false;
				watcher.Dispose();
				watcher = null;
			}
			if (debounceTimer != null)
			{
				debounceTimer.Stop();
				debounceTimer.Dispose();
				debounceTimer = null;
			}
		}

		public static DialogResult ShowConfigure(IWin32Window owner, string packageDirectory, HostJsonRpcContext context)
		{
			if (!PluginManifest.TryLoad(packageDirectory, out PluginManifest manifest, out string error))
			{
				MessageBox.Show(owner, error, "Plugin Configure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return DialogResult.Cancel;
			}
			string html = manifest.ResolveConfigureEntry(packageDirectory);
			if (html == null)
			{
				MessageBox.Show(owner, "plugin.json ui.configure entry is missing or invalid.", "Plugin Configure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return DialogResult.Cancel;
			}
			context.PluginId = manifest.Id;
			context.PluginVersion = manifest.Version ?? string.Empty;
			context.CommandPath = packageDirectory;
			context.HostApiVersion = PluginManifest.SupportedApiVersion;
			using (var form = new WebViewPluginForm(packageDirectory, html, manifest, context))
			{
				return form.ShowDialog(owner);
			}
		}
	}
}
