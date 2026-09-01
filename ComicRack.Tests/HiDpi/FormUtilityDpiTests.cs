using System;
using cYo.Common.Windows.Forms;
using Xunit;

namespace ComicRack.Tests.HiDpi
{
	public class FormUtilityDpiTests
	{
		[Fact]
		public void RefreshDpiScale_RaisesDpiScaleChangedWithSource()
		{
			DpiScaleChangedEventArgs captured = null;
			EventHandler<DpiScaleChangedEventArgs> handler = (_, e) => captured = e;
			FormUtility.DpiScaleChanged += handler;

			try
			{
				FormUtility.RefreshDpiScale(null);
			}
			finally
			{
				FormUtility.DpiScaleChanged -= handler;
				FormUtility.RefreshDpiScale(null);
			}

			Assert.NotNull(captured);
			Assert.Null(captured.Source);
			Assert.True(captured.Scale.X > 0f);
			Assert.True(captured.Scale.Y > 0f);
		}

		[Fact]
		public void ScaleDpiY_And_UnscaleDpiY_Are_Inverses_AtCachedScale()
		{
			FormUtility.RefreshDpiScale(null);
			float scale = FormUtility.DpiScale.Y;
			if (scale <= 0f)
			{
				return;
			}

			const int logical = 128;
			int scaled = FormUtility.ScaleDpiY(logical);
			int roundTrip = FormUtility.UnscaleDpiY(scaled);
			Assert.Equal(logical, roundTrip);
		}
	}
}
