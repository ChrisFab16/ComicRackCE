using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using cYo.Common.Windows.Forms;
using Xunit;

namespace ComicRack.Tests.HiDpi
{
	public class ItemViewConfigPersistenceTests
	{
		[Fact]
		public void XmlRoundTrip_PreservesThumbnailSize()
		{
			var original = new ItemViewConfig
			{
				ThumbnailSize = new Size(128, 128),
				ItemRowHeight = 32
			};

			var serializer = new XmlSerializer(typeof(ItemViewConfig));
			ItemViewConfig restored;
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(stream, original);
				stream.Position = 0;
				restored = (ItemViewConfig)serializer.Deserialize(stream);
			}

			Assert.Equal(128, restored.ThumbnailSize.Height);
			Assert.Equal(128, restored.ThumbnailSize.Width);
			Assert.Equal(32, restored.ItemRowHeight);
		}
	}
}
