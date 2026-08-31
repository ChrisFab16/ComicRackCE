using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using cYo.Common.Collections;
using cYo.Common.Drawing;
using cYo.Common.Localize;
using cYo.Common.Windows;
using cYo.Common.Windows.Forms;
using cYo.Common.Windows.Forms.Theme;
using cYo.Projects.ComicRack.Viewer.Config;
using cYo.Projects.ComicRack.Viewer.Controls;
using cYo.Projects.ComicRack.Viewer.Properties;

namespace cYo.Projects.ComicRack.Viewer.Views
{
	public partial class ComicListFolderFilesBrowser : ComicListFilesBrowser, IDisplayWorkspace
	{
		private const int DesignFavoritesExpandedHeight = 160;

        private readonly CommandMapper commands = new CommandMapper();

		private readonly NiceTreeSkin folderTreeSkin;

		private readonly Dictionary<ToolStripButton, Image> toolbarOriginalImages = new Dictionary<ToolStripButton, Image>();

		private string cachedCurrentFolder = string.Empty;

		private SmartList<string> paths;

		private bool initialEnter = true;


		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentFolder
		{
			get
			{
				if (tvFolders == null || tvFolders.Nodes.Count <= 0)
				{
					return cachedCurrentFolder;
				}
				return tvFolders.GetSelectedNodePath();
			}
			set
			{
				if (tvFolders == null || tvFolders.Nodes.Count == 0)
				{
					cachedCurrentFolder = value;
					return;
				}
				tvFolders.DrillToFolder(value);
				tvFolders.SelectedNode.EnsureVisible();
				cachedCurrentFolder = string.Empty;
			}
		}

		public SmartList<string> Paths
		{
			get
			{
				return paths;
			}
			set
			{
				if (paths != value)
				{
					if (paths != null)
					{
						paths.Changed -= paths_Changed;
					}
					paths = value;
					if (paths != null)
					{
						paths.Changed += paths_Changed;
					}
				}
			}
		}

		public override bool TopBrowserVisible
		{
			get
			{
				return favContainer.Expanded;
			}
			set
			{
				favContainer.Expanded = value;
			}
		}

		public override int TopBrowserSplit
		{
			get
			{
				return favContainer.ExpandedWidth;
			}
			set
			{
				favContainer.ExpandedWidth = value;
			}
		}

		public ComicListFolderFilesBrowser()
		{
			InitializeComponent();
			tvFolders.SortNetworkFolders = Program.ExtendedSettings.SortNetworkFolders;
			folderTreeSkin = new NiceTreeSkin
			{
				TreeView = tvFolders
			};
			tvFolders.SetSidePanelColor();
			folderTreeSkin.TreeView.SetSidePanelColor();
			CaptureToolbarOriginalImages();
			FormUtility.DpiScaleChanged += OnDpiScaleChanged;
			components.Add(commands);
			LocalizeUtility.Localize(this, components);
			ApplyFoldersSidebarMetrics();
        }

		public ComicListFolderFilesBrowser(SmartList<string> paths)
			: this()
		{
			Paths = paths;
		}

		public void ApplyFoldersSidebarMetrics()
		{
			if (base.DesignMode || tvFolders == null)
			{
				return;
			}
			tvFolders.Font = SystemFonts.MessageBoxFont;
			tvFolders.Indent = FormUtility.ScaleDpiX(15);
			tvFolders.ItemHeight = tvFolders.Font.Height + FormUtility.ScaleDpiY(8);
			favContainer.Padding = new Padding(0, FormUtility.ScaleDpiY(6), 0, 0);
			favContainer.GripWidth = FormUtility.ScaleDpiX(6);
			if (favContainer.ExpandedWidth <= DesignFavoritesExpandedHeight)
			{
				favContainer.ExpandedWidth = FormUtility.ScaleDpiY(DesignFavoritesExpandedHeight);
			}
			ApplyFavoritesTileSize();
			ApplyToolbarMetrics();
			tvFolders.Invalidate();
			favView.Invalidate();
		}

		protected virtual void OnInitalDisplay()
		{
			this.BeginInvoke(delegate
			{
				tvFolders.Init();
				Update();
				if (!string.IsNullOrEmpty(cachedCurrentFolder))
				{
					CurrentFolder = cachedCurrentFolder;
				}
			});
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!base.DesignMode)
			{
				ApplyFoldersSidebarMetrics();
				commands.Add(OpenWindow, miOpenWindow, tbOpenWindow);
				commands.Add(OpenTab, miOpenTab, tbOpenTab);
				commands.Add(AddToFavorites, miAddToFavorites);
				commands.Add(RefreshDisplay, tbRefresh, miRefresh);
				commands.Add(delegate
				{
					Program.Scanner.ScanFileOrFolder(CurrentFolder, all: true, removeMissing: false);
				}, miAddFolderLibrary);
				commands.Add(base.SwitchIncludeSubFolders, true, () => base.IncludeSubFolders, tbIncludeSubFolders);
				commands.Add(delegate
				{
					TopBrowserVisible = !TopBrowserVisible;
				}, true, () => TopBrowserVisible, tbFavorites);
				commands.Add(RefreshFavorites, miFavRefresh);
				commands.Add(RemoveFavorite, miRemove);
				CurrentFolder = Program.Settings.LastExplorerFolder;
				base.IncludeSubFolders = Program.Settings.ExplorerIncludeSubFolders;
				FillFavorites();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (initialEnter)
			{
				initialEnter = false;
				OnInitalDisplay();
			}
		}

		protected override void OnRefreshDisplay()
		{
			base.OnRefreshDisplay();
			using (new WaitCursor())
			{
				string currentFolder = CurrentFolder;
				tvFolders.Init();
				tvFolders.DrillToFolder(currentFolder);
			}
		}

		private void OnDpiScaleChanged(object sender, EventArgs e)
		{
			if (!base.IsHandleCreated || base.DesignMode)
			{
				return;
			}
			if (InvokeRequired)
			{
				BeginInvoke(new MethodInvoker(ApplyFoldersSidebarMetrics));
				return;
			}
			ApplyFoldersSidebarMetrics();
		}

		private void CaptureToolbarOriginalImages()
		{
			toolbarOriginalImages.Clear();
			RegisterToolbarOriginal(tbFavorites);
			RegisterToolbarOriginal(tbIncludeSubFolders);
			RegisterToolbarOriginal(tbOpenWindow);
			RegisterToolbarOriginal(tbOpenTab);
			RegisterToolbarOriginal(tbRefresh);
		}

		private void RegisterToolbarOriginal(ToolStripButton button)
		{
			if (button?.Image != null)
			{
				toolbarOriginalImages[button] = button.Image;
			}
		}

		private void ApplyToolbarMetrics()
		{
			if (toolStrip == null)
			{
				return;
			}
			toolStrip.Height = FormUtility.ScaleDpiY(25);
			Size buttonSize = new Size(23, 22).ScaleDpi();
			foreach (ToolStripItem item in toolStrip.Items)
			{
				if (item is ToolStripButton button)
				{
					button.Size = buttonSize;
					if (toolbarOriginalImages.TryGetValue(button, out Image original) && original is Bitmap bitmap)
					{
						button.Image = bitmap.ScaleDpi();
					}
				}
			}
		}

		private void ApplyFavoritesTileSize()
		{
			if (favView == null)
			{
				return;
			}
			int width = favView.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth - FormUtility.ScaleDpiX(8);
			if (width <= 0)
			{
				width = Math.Max(0, favView.Width - FormUtility.ScaleDpiX(8));
			}
			Font font = favView.Font ?? SystemFonts.MessageBoxFont;
			int lineHeight = font.Height;
			int height = Math.Max(FormUtility.ScaleDpiY(50), 2 * lineHeight + FormUtility.ScaleDpiY(12));
			favView.ItemTileSize = new Size(width, height);
		}

		private void tvFolders_AfterSelect(object sender, TreeViewEventArgs e)
		{
			FillBooks(CurrentFolder);
		}

		private void paths_Changed(object sender, SmartListChangedEventArgs<string> e)
		{
			FillFavorites();
		}

		private void favView_SelectedIndexChanged(object sender, EventArgs e)
		{
			ItemViewItem itemViewItem = favView.FocusedItem as ItemViewItem;
			if (itemViewItem != null)
			{
				CurrentFolder = itemViewItem.Tag as string;
			}
		}

		private void favView_Resize(object sender, EventArgs e)
		{
			ApplyFavoritesTileSize();
		}

		private void FillFavorites()
		{
			FillFavorites(refreshThumbnails: false);
		}

		private void FillFavorites(bool refreshThumbnails)
		{
			using (new WaitCursor())
			{
				if (Program.Settings == null)
				{
					return;
				}
				List<string> list = new List<string>();
				favView.Items.Clear();
				foreach (string favoriteFolder in Program.Settings.FavoriteFolders)
				{
					if (!Directory.Exists(favoriteFolder))
					{
						list.Add(favoriteFolder);
						continue;
					}
					FolderViewItem folderViewItem = FolderViewItem.Create(favoriteFolder);
					folderViewItem.Tag = favoriteFolder;
					folderViewItem.TooltipText = favoriteFolder;
					favView.Items.Add(folderViewItem);
					if (refreshThumbnails)
					{
						Program.ImagePool.Thumbs.RefreshImage(folderViewItem.ThumbnailKey);
					}
				}
				Program.Settings.FavoriteFolders.RemoveRange(list);
			}
		}

		private void RemoveFavorite()
		{
			ItemViewItem itemViewItem = ((favView.FocusedItem == null) ? null : (favView.FocusedItem as ItemViewItem));
			if (itemViewItem != null && Program.AskQuestion(this, TR.Messages["AskRemoveFavorite", "Do you really want to remove this Favorite Folder link?"], TR.Messages["Remove", "Remove"], HiddenMessageBoxes.RemoveFavorite))
			{
				Program.Settings.FavoriteFolders.Remove(itemViewItem.Tag as string);
				favView.Items.Remove(itemViewItem);
			}
		}

		private void RefreshFavorites()
		{
			FillFavorites(refreshThumbnails: true);
		}

		private void AddToFavorites()
		{
			if (!Program.Settings.FavoriteFolders.Contains(CurrentFolder))
			{
				Program.Settings.FavoriteFolders.Add(CurrentFolder);
			}
		}

		private void OpenWindow()
		{
			try
			{
				OpenListInNewWindow();
			}
			catch
			{
			}
		}

		private void OpenTab()
		{
			try
			{
				OpenListInNewTab(Resources.SearchFolder);
			}
			catch
			{
			}
		}

		public void SetWorkspace(DisplayWorkspace workspace)
		{
		}

		public void StoreWorkspace(DisplayWorkspace workspace)
		{
			Program.Settings.ExplorerIncludeSubFolders = base.IncludeSubFolders;
			Program.Settings.LastExplorerFolder = CurrentFolder;
		}
	}
}
