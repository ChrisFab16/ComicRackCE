using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using cYo.Common.Windows.Forms;

namespace cYo.Projects.ComicRack.Viewer.Views
{
	[Serializable]
	public class ComicExplorerViewSettings
	{
		[XmlAttribute]
		[DefaultValue(true)]
		public bool ShowBrowser
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(150)]
		public int BrowserSplit
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(150)]
		public int PreviewSplit
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(150)]
		public int TopBrowserSplit
		{
			get;
			set;
		}

		[DefaultValue(typeof(Size), "200, 150")]
		public Size InfoBrowserSize
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool InfoBrowserRight
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ShowPreview
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ShowInfo
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ShowTopBrowser
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ShowSearchBrowser
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ShowSearchBar
		{
			get;
			set;
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool TwoPagePreview
		{
			get;
			set;
		}

		[DefaultValue(null)]
		public ItemViewConfig ItemViewConfig
		{
			get;
			set;
		}

		[DefaultValue(1)]
		public int SearchBrowserColumn1
		{
			get;
			set;
		}

		[DefaultValue(0)]
		public int SearchBrowserColumn2
		{
			get;
			set;
		}

		[DefaultValue(2)]
		public int SearchBrowserColumn3
		{
			get;
			set;
		}

		public ComicExplorerViewSettings()
		{
			SearchBrowserColumn3 = 2;
			SearchBrowserColumn1 = 1;
			TopBrowserSplit = FormUtility.ScaleDpiY(150);
			PreviewSplit = FormUtility.ScaleDpiY(200);
			BrowserSplit = FormUtility.ScaleDpiY(250);
			InfoBrowserSize = new Size(200, 150).ScaleDpi();
			ShowBrowser = true;
		}

		public static ComicExplorerViewSettings NormalizeLegacySplits(ComicExplorerViewSettings settings)
		{
			if (settings == null)
			{
				return new ComicExplorerViewSettings();
			}
			try
			{
				if (FormUtility.DpiScale.Y <= 1.01f)
				{
					return settings;
				}
				if (settings.TopBrowserSplit <= 0)
				{
					settings.TopBrowserSplit = new ComicExplorerViewSettings().TopBrowserSplit;
				}
				else if (settings.TopBrowserSplit <= 150)
				{
					settings.TopBrowserSplit = FormUtility.ScaleDpiY(settings.TopBrowserSplit);
				}
				if (settings.BrowserSplit <= 0)
				{
					settings.BrowserSplit = new ComicExplorerViewSettings().BrowserSplit;
				}
				else if (settings.BrowserSplit <= 250)
				{
					settings.BrowserSplit = FormUtility.ScaleDpiY(settings.BrowserSplit);
				}
				if (settings.PreviewSplit <= 0)
				{
					settings.PreviewSplit = new ComicExplorerViewSettings().PreviewSplit;
				}
				else if (settings.PreviewSplit <= 200)
				{
					settings.PreviewSplit = FormUtility.ScaleDpiY(settings.PreviewSplit);
				}
				Size infoSize = settings.InfoBrowserSize;
				if (infoSize.Width <= 0 || infoSize.Height <= 0)
				{
					settings.InfoBrowserSize = new ComicExplorerViewSettings().InfoBrowserSize;
				}
				else if (infoSize.Width <= 200 && infoSize.Height <= 150)
				{
					settings.InfoBrowserSize = infoSize.ScaleDpi();
				}
				return settings;
			}
			catch
			{
				return new ComicExplorerViewSettings();
			}
		}
	}
}
