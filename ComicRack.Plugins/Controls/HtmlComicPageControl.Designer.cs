using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace cYo.Projects.ComicRack.Plugins.Controls
{
	public partial class HtmlComicPageControl
	{
		private IContainer components = null;
		private WebView2 webView;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (SaveConfigFunction != null && originalConfig != ScriptConfig)
				{
					SaveConfigFunction(ScriptConfig);
				}
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			webView = new WebView2();
			SuspendLayout();
			webView.Dock = DockStyle.Fill;
			webView.Location = new Point(0, 0);
			webView.Name = "webView";
			webView.Size = new Size(540, 402);
			webView.TabIndex = 1;
			AutoScaleDimensions = new SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(webView);
			Name = "HtmlComicPageControl";
			Size = new Size(540, 402);
			ResumeLayout(false);
		}
	}
}
