using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cYo.Projects.ComicRack.Plugins.WebView
{
	public sealed class PluginManifest
	{
		public const int SupportedApiVersion = 1;

		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("version")]
		public string Version { get; set; }

		[JsonPropertyName("apiVersion")]
		public int ApiVersion { get; set; }

		[JsonPropertyName("kind")]
		public string Kind { get; set; }

		[JsonPropertyName("ui")]
		public PluginManifestUi Ui { get; set; }

		[JsonPropertyName("hooks")]
		public List<PluginManifestHook> Hooks { get; set; }

		public static string ManifestFileName => "plugin.json";

		public static bool TryLoad(string packageDirectory, out PluginManifest manifest, out string error)
		{
			manifest = null;
			error = null;
			if (string.IsNullOrEmpty(packageDirectory))
			{
				error = "Package directory is empty.";
				return false;
			}
			string path = Path.Combine(packageDirectory, ManifestFileName);
			if (!File.Exists(path))
			{
				error = "plugin.json not found.";
				return false;
			}
			try
			{
				string json = File.ReadAllText(path);
				manifest = JsonSerializer.Deserialize<PluginManifest>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true
				});
			}
			catch (Exception ex)
			{
				error = "Invalid plugin.json: " + ex.Message;
				return false;
			}
			if (manifest == null || string.IsNullOrWhiteSpace(manifest.Id))
			{
				error = "plugin.json must declare a non-empty id.";
				return false;
			}
			if (manifest.ApiVersion < 1)
			{
				error = "plugin.json apiVersion must be >= 1.";
				return false;
			}
			if (manifest.ApiVersion > SupportedApiVersion)
			{
				error = $"plugin.json apiVersion {manifest.ApiVersion} is newer than host support ({SupportedApiVersion}).";
				return false;
			}
			return true;
		}

		public string ResolveConfigureEntry(string packageDirectory)
		{
			string relative = Ui?.Configure;
			if (string.IsNullOrWhiteSpace(relative))
			{
				return null;
			}
			string full = Path.GetFullPath(Path.Combine(packageDirectory, relative));
			string root = Path.GetFullPath(packageDirectory);
			if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				root += Path.DirectorySeparatorChar;
			}
			if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			return File.Exists(full) ? full : null;
		}

		public bool HotReloadEnabled => Ui?.HotReload != false;
	}

	public sealed class PluginManifestUi
	{
		[JsonPropertyName("configure")]
		public string Configure { get; set; }

		[JsonPropertyName("hotReload")]
		public bool? HotReload { get; set; }
	}

	public sealed class PluginManifestHook
	{
		[JsonPropertyName("type")]
		public string Type { get; set; }

		[JsonPropertyName("key")]
		public string Key { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }
	}
}
