using System.Drawing;

namespace cYo.Projects.ComicRack.Engine.IO
{
	/// <summary>
	/// Process-wide registration for the active page-image filter (v1: at most one).
	/// Default off. Dev identity filter used for pipeline spike only.
	/// </summary>
	public static class PageImageFilterHost
	{
		private static IPageImageFilter active;

		public static IPageImageFilter Active
		{
			get => active;
			set => active = value;
		}

		/// <summary>Fingerprint for PageKey cache identity; empty when no filter or disabled.</summary>
		public static string CurrentFingerprint
		{
			get
			{
				IPageImageFilter f = active;
				if (f == null || !f.IsEnabled)
				{
					return string.Empty;
				}
				string fp = f.Fingerprint;
				return fp ?? string.Empty;
			}
		}

		/// <summary>Enable a no-op clone filter for ImagePool pipeline spike (T007/T008).</summary>
		public static void EnableDevIdentityFilter(bool enabled)
		{
			if (!enabled)
			{
				if (active is DevIdentityPageImageFilter)
				{
					active = null;
				}
				return;
			}
			active = new DevIdentityPageImageFilter();
		}

		private sealed class DevIdentityPageImageFilter : IPageImageFilter
		{
			public string Fingerprint => "dev-identity:1";

			public bool IsEnabled => true;

			public Bitmap Apply(Bitmap source)
			{
				if (source == null)
				{
					return null;
				}
				// Clone so cache keys / ownership behave like a real filter output.
				return (Bitmap)source.Clone();
			}
		}
	}
}
