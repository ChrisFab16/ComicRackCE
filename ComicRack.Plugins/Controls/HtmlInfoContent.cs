using System;

namespace cYo.Projects.ComicRack.Plugins.Controls
{
	public static class HtmlInfoContent
	{
		public static bool IsUrlResult(string text, out string url)
		{
			url = null;
			if (string.IsNullOrEmpty(text) || text[0] != '!')
			{
				return false;
			}
			url = text.Substring(1);
			return !string.IsNullOrWhiteSpace(url);
		}
	}
}
