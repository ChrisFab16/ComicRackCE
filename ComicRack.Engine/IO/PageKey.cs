using System;
using cYo.Common.Drawing;

namespace cYo.Projects.ComicRack.Engine.IO
{
	[Serializable]
	public class PageKey : ImageKey
	{
		private BitmapAdjustment adjustment = BitmapAdjustment.Empty;
		private string filterFingerprint = string.Empty;

		public BitmapAdjustment Adjustment { get => adjustment; set => adjustment = value; }

		/// <summary>Display-filter cache identity (empty = stock BitmapAdjustment-only path).</summary>
		public string FilterFingerprint
		{
			get => filterFingerprint ?? string.Empty;
			set => filterFingerprint = value ?? string.Empty;
		}

		public PageKey(object source, string location, long size, DateTime modified, int index, ImageRotation rotation, BitmapAdjustment adjustment)
			: base(source, location, size, modified, index, rotation)
		{
			this.adjustment = adjustment;
			this.filterFingerprint = PageImageFilterHost.CurrentFingerprint;
		}

		public PageKey(object source, string location, long size, DateTime modified, int index, ImageRotation rotation, BitmapAdjustment adjustment, string filterFingerprint)
			: base(source, location, size, modified, index, rotation)
		{
			this.adjustment = adjustment;
			this.filterFingerprint = filterFingerprint ?? string.Empty;
		}

		public PageKey(ImageKey key)
			: base(key)
		{
			if (key is PageKey pk)
			{
				adjustment = pk.adjustment;
				filterFingerprint = pk.FilterFingerprint;
			}
		}

		public PageKey()
			: base()
		{		
		}

		protected override int CreateHashCode()
		{
			int h = base.CreateHashCode() ^ adjustment.GetHashCode();
			string fp = FilterFingerprint;
			if (!string.IsNullOrEmpty(fp))
			{
				h ^= fp.GetHashCode();
			}
			return h;
		}

		public override bool Equals(object obj)
		{
			PageKey pageKey = obj as PageKey;
			if (base.Equals(obj) && pageKey != null)
			{
				return pageKey.adjustment == adjustment
					&& string.Equals(pageKey.FilterFingerprint, FilterFingerprint, StringComparison.Ordinal);
			}
			return false;
		}
	}
}
