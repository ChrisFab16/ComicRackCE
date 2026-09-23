using System;
using System.Collections.Generic;
using System.IO;
using cYo.Projects.ComicRack.Plugins.WebView;

namespace cYo.Projects.ComicRack.Plugins
{
	/// <summary>
	/// Discovers <c>kind: web</c> packages from <c>plugin.json</c> (no .py required).
	/// </summary>
	public class JsonPluginInitializer : PluginInitializer
	{
		public override IEnumerable<Command> GetCommands(string file)
		{
			var list = new List<Command>();
			if (string.IsNullOrEmpty(file) ||
			    !string.Equals(Path.GetFileName(file), PluginManifest.ManifestFileName, StringComparison.OrdinalIgnoreCase))
			{
				return list;
			}

			string packageDir = Path.GetDirectoryName(file);
			if (!PluginManifest.TryLoad(packageDir, out PluginManifest manifest, out _))
			{
				return list;
			}

			if (!string.Equals(manifest.Kind, "web", StringComparison.OrdinalIgnoreCase))
			{
				return list;
			}

			if (manifest.ResolveConfigureEntry(packageDir) == null)
			{
				return list;
			}

			string key = string.IsNullOrWhiteSpace(manifest.Id) ? Path.GetFileName(packageDir) : manifest.Id;
			bool addedPrimary = false;

			if (manifest.Hooks != null)
			{
				foreach (PluginManifestHook hook in manifest.Hooks)
				{
					if (hook == null || string.IsNullOrWhiteSpace(hook.Type))
					{
						continue;
					}
					if (string.Equals(hook.Type, PluginEngine.ScriptTypeConfig, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					list.Add(CreateCommand(
						hook.Type,
						string.IsNullOrWhiteSpace(hook.Key) ? key : hook.Key,
						string.IsNullOrWhiteSpace(hook.Name) ? key : hook.Name,
						opensConfigure: true));
					addedPrimary = true;
				}
			}

			if (!addedPrimary)
			{
				list.Add(CreateCommand(PluginEngine.ScriptTypeBooks, key, key, opensConfigure: true));
			}

			list.Add(CreateCommand(PluginEngine.ScriptTypeConfig, key, key + " Configure", opensConfigure: true));
			return list;
		}

		private static WebPluginCommand CreateCommand(string hook, string key, string name, bool opensConfigure)
		{
			return new WebPluginCommand
			{
				Hook = hook,
				Key = key,
				Name = name,
				Description = "Web plugin (Host API SPA)",
				Enabled = true,
				OpensConfigure = opensConfigure
			};
		}
	}
}
