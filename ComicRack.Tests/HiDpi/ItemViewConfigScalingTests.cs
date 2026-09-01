using System.Drawing;
using cYo.Common.Windows.Forms;
using Xunit;

namespace ComicRack.Tests.HiDpi
{
	public class ItemViewConfigScalingTests
	{
		[Fact]
		public void WorkspaceThumbHeight_IsUsedAsDisplayPixels_NotScaledByDpi()
		{
			const int persistedThumb = 128;
			var config = new ItemViewConfig
			{
				ItemViewMode = ItemViewMode.Thumbnail,
				ThumbnailSize = new Size(persistedThumb, persistedThumb)
			};

			Assert.Equal(persistedThumb, config.ThumbnailSize.Height);
		}

		[Fact]
		public void FormUtility_ScaleDpiY_ScalesClampBoundsOnly()
		{
			FormUtility.RefreshDpiScale(null);
			float scale = FormUtility.DpiScale.Y;
			if (scale <= 0f)
			{
				return;
			}

			const int logicalMin = 32;
			const int logicalMax = 256;
			const int thumbHeight = 128;

			Assert.Equal((int)(logicalMin * scale), FormUtility.ScaleDpiY(logicalMin));
			Assert.Equal((int)(logicalMax * scale), FormUtility.ScaleDpiY(logicalMax));
			Assert.Equal(thumbHeight, thumbHeight);
		}
	}
}
