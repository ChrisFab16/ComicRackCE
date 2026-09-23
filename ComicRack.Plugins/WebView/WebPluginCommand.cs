using cYo.Projects.ComicRack.Plugins.WebView;

namespace cYo.Projects.ComicRack.Plugins
{
	/// <summary>
	/// Managed command for <c>kind: web</c> packages (no IronPython).
	/// </summary>
	public class WebPluginCommand : Command
	{
		/// <summary>
		/// When true, invoke opens the package WebView2 Configure SPA.
		/// </summary>
		public bool OpensConfigure { get; set; } = true;

		protected override object OnInvoke(object[] data)
		{
			if (Environment == null)
			{
				return null;
			}
			if (OpensConfigure || IsHook(PluginEngine.ScriptTypeConfig))
			{
				WebViewPluginHost.ShowConfigure(Environment, Environment.CommandPath);
			}
			return null;
		}
	}
}
