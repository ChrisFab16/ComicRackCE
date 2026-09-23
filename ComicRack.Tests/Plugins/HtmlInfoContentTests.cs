using cYo.Projects.ComicRack.Plugins.Controls;
using Xunit;

namespace ComicRack.Tests.Plugins
{
	public class HtmlInfoContentTests
	{
		[Fact]
		public void IsUrlResult_DetectsBangPrefix()
		{
			Assert.True(HtmlInfoContent.IsUrlResult("!https://example.com", out string url));
			Assert.Equal("https://example.com", url);
		}

		[Fact]
		public void IsUrlResult_HtmlIsNotUrl()
		{
			Assert.False(HtmlInfoContent.IsUrlResult("<html></html>", out string url));
			Assert.Null(url);
		}

		[Fact]
		public void IsUrlResult_EmptyBang_False()
		{
			Assert.False(HtmlInfoContent.IsUrlResult("!", out _));
		}
	}
}
