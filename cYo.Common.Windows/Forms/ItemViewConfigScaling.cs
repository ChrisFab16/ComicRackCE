using System;
using System.Drawing;
using System.Windows.Forms;

namespace cYo.Common.Windows.Forms
{
	public static class ItemViewConfigScaling
	{
		public static ItemViewConfig ToLogical(ItemViewConfig config)
		{
			if (config == null)
			{
				return null;
			}
			config.ThumbnailSize = config.ThumbnailSize.UnscaleDpi();
			config.TileSize = config.TileSize.UnscaleDpi();
			config.ItemRowHeight = FormUtility.UnscaleDpiY(config.ItemRowHeight);
			return config;
		}

		public static void ApplyLogicalDisplaySizes(ItemViewConfig logical, Action<int> setItemSize)
		{
			ApplyLogicalDisplaySizes(logical, setItemSize, FormUtility.DpiScale.Y);
		}

		public static void ApplyLogicalDisplaySizes(ItemViewConfig logical, Action<int> setItemSize, float dpiScaleY)
		{
			if (logical == null || setItemSize == null || dpiScaleY <= 0f)
			{
				return;
			}
			switch (logical.ItemViewMode)
			{
				case ItemViewMode.Thumbnail:
					if (logical.ThumbnailSize.Height >= 16)
					{
						setItemSize(ScaleLogical(logical.ThumbnailSize.Height, dpiScaleY));
					}
					break;
				case ItemViewMode.Tile:
					if (logical.TileSize.Height >= 16)
					{
						setItemSize(ScaleLogical(logical.TileSize.Height, dpiScaleY));
					}
					break;
				case ItemViewMode.Detail:
					if (logical.ItemRowHeight >= 8)
					{
						setItemSize(ScaleLogical(logical.ItemRowHeight, dpiScaleY));
					}
					break;
			}
		}

		public static int ScaleLogical(int logical, float dpiScaleY)
		{
			return (int)(logical * dpiScaleY);
		}

		public static int UnscaleDisplay(int display, float dpiScaleY)
		{
			return (int)(display / dpiScaleY);
		}
	}
}
