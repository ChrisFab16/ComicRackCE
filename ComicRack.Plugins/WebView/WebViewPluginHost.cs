using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using cYo.Projects.ComicRack.Engine;

namespace cYo.Projects.ComicRack.Plugins.WebView
{
	/// <summary>
	/// Opens SPA Configure dialogs and optional selected-book provider registration from the host app.
	/// </summary>
	public static class WebViewPluginHost
	{
		public static Func<IEnumerable<ComicBook>> SelectedBooksProvider { get; set; }

		public static DialogResult ShowConfigure(IPluginEnvironment env, string packageDirectory = null)
		{
			if (env == null)
			{
				throw new ArgumentNullException(nameof(env));
			}
			string dir = packageDirectory ?? env.CommandPath;
			if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
			{
				MessageBox.Show(env.MainWindow, "Plugin package directory is not set.", "Plugin Configure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return DialogResult.Cancel;
			}

			string configPath = Path.Combine(dir, "webview-ui.config");
			var context = new HostJsonRpcContext
			{
				ProductVersion = env.App?.ProductVersion ?? string.Empty,
				CommandPath = dir,
				GetLibraryBooks = () => env.App?.GetLibraryBooks() ?? Enumerable.Empty<ComicBook>(),
				GetSelectedBooks = () => SelectedBooksProvider?.Invoke() ?? Enumerable.Empty<ComicBook>(),
				GetTheme = () =>
				{
					Color back = SystemColors.Window;
					Color fore = SystemColors.WindowText;
					bool dark = back.GetBrightness() < 0.5f;
					try
					{
						if (env.MainWindow is Control c)
						{
							back = c.BackColor;
							fore = c.ForeColor;
							dark = back.GetBrightness() < 0.5f;
						}
					}
					catch
					{
					}
					return (dark, back, fore);
				},
				AskQuestion = (q, b, o) => env.App?.AskQuestion(q, b, o) ?? 0,
				ShowComicInfo = books => env.App?.ShowComicInfo(books),
				LoadConfig = () =>
				{
					try
					{
						return File.Exists(configPath) ? File.ReadAllText(configPath) : string.Empty;
					}
					catch
					{
						return string.Empty;
					}
				},
				SaveConfig = text =>
				{
					try
					{
						File.WriteAllText(configPath, text ?? string.Empty);
						return true;
					}
					catch
					{
						return false;
					}
				}
			};

			return WebViewPluginForm.ShowConfigure(env.MainWindow, dir, context);
		}
	}
}
