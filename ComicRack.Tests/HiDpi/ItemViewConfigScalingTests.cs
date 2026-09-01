using System.Drawing;
using System.Windows.Forms;
using cYo.Common.Windows.Forms;
using Xunit;

namespace ComicRack.Tests.HiDpi
{
	public class ItemViewConfigScalingTests
	{
		private const float Scale150 = 1.5f;

		[Fact]
		public void LogicalThumb128_At150PercentScale_Displays192()
		{
			Assert.Equal(192, ItemViewConfigScaling.ScaleLogical(128, Scale150));
			Assert.Equal(128, ItemViewConfigScaling.UnscaleDisplay(192, Scale150));
		}

		[Fact]
		public void ApplyLogicalDisplaySizes_ThumbnailMode_UsesLogicalHeightAt150Percent()
		{
			int captured = 0;
			var logical = new ItemViewConfig
			{
				ItemViewMode = ItemViewMode.Thumbnail,
				ThumbnailSize = new Size(128, 128)
			};

			ItemViewConfigScaling.ApplyLogicalDisplaySizes(logical, height => captured = height, Scale150);

			Assert.Equal(192, captured);
		}

		[Fact]
		public void ToLogical_UnscalesDisplaySizedConfig()
		{
			FormUtility.RefreshDpiScale(null);
			float scale = FormUtility.DpiScale.Y;
			if (scale <= 0f)
			{
				return;
			}

			const int logicalThumb = 128;
			const int logicalRow = 32;
			var display = new ItemViewConfig
			{
				ItemViewMode = ItemViewMode.Thumbnail,
				ThumbnailSize = new Size(
					ItemViewConfigScaling.ScaleLogical(logicalThumb, scale),
					ItemViewConfigScaling.ScaleLogical(logicalThumb, scale)),
				ItemRowHeight = ItemViewConfigScaling.ScaleLogical(logicalRow, scale)
			};

			ItemViewConfigScaling.ToLogical(display);

			Assert.Equal(logicalThumb, display.ThumbnailSize.Height);
			Assert.Equal(logicalRow, display.ItemRowHeight);
		}

		[Fact]
		public void ApplyLogicalDisplaySizes_DetailMode_SetsScaledRowHeight()
		{
			int captured = 0;
			var logical = new ItemViewConfig
			{
				ItemViewMode = ItemViewMode.Detail,
				ItemRowHeight = 32
			};

			ItemViewConfigScaling.ApplyLogicalDisplaySizes(logical, height => captured = height, Scale150);

			Assert.Equal(48, captured);
		}
	}
}
