using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using cYo.Projects.ComicRack.Engine;

namespace cYo.Projects.ComicRack.Plugins.WebView
{
	public sealed class HostJsonRpcContext
	{
		public int HostApiVersion { get; set; } = PluginManifest.SupportedApiVersion;

		public string ProductVersion { get; set; }

		public string PluginId { get; set; }

		public string PluginVersion { get; set; }

		public string CommandPath { get; set; }

		public Func<IEnumerable<ComicBook>> GetLibraryBooks { get; set; }

		public Func<IEnumerable<ComicBook>> GetSelectedBooks { get; set; }

		public Func<(bool isDark, Color back, Color fore)> GetTheme { get; set; }

		public Func<string, string, string, int> AskQuestion { get; set; }

		public Action<IEnumerable<ComicBook>> ShowComicInfo { get; set; }

		public Func<string> LoadConfig { get; set; }

		public Func<string, bool> SaveConfig { get; set; }

		public Action ReloadUi { get; set; }

		public Action<string> CloseUi { get; set; }
	}

	public static class HostJsonRpc
	{
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public static string HandleMessage(string json, HostJsonRpcContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}
			JsonNode root;
			try
			{
				root = JsonNode.Parse(json);
			}
			catch
			{
				return Error(null, -32700, "Parse error");
			}
			if (root == null || root["method"] == null)
			{
				return Error(root?["id"], -32600, "Invalid Request");
			}
			JsonNode id = root["id"];
			string method = root["method"]?.GetValue<string>();
			JsonNode parms = root["params"];
			try
			{
				object result = Dispatch(method, parms, context);
				return Success(id, result);
			}
			catch (HostJsonRpcException ex)
			{
				return Error(id, ex.Code, ex.Message);
			}
			catch (Exception ex)
			{
				return Error(id, -32000, ex.Message);
			}
		}

		private static object Dispatch(string method, JsonNode parms, HostJsonRpcContext context)
		{
			switch (method)
			{
				case "host.getInfo":
					return new
					{
						productVersion = context.ProductVersion ?? string.Empty,
						apiVersion = context.HostApiVersion,
						pluginId = context.PluginId ?? string.Empty,
						pluginVersion = context.PluginVersion ?? string.Empty,
						commandPath = context.CommandPath ?? string.Empty
					};
				case "host.getTheme":
				{
					var theme = context.GetTheme?.Invoke() ?? (false, SystemColors.Window, SystemColors.WindowText);
					return new
					{
						isDark = theme.Item1,
						backColor = ToHex(theme.Item2),
						foreColor = ToHex(theme.Item3)
					};
				}
				case "host.getSelectedBooks":
					return new { books = MapBooks(context.GetSelectedBooks?.Invoke(), Limit(parms)) };
				case "host.getLibraryBooks":
					return new { books = MapBooks(context.GetLibraryBooks?.Invoke(), Limit(parms)) };
				case "host.askQuestion":
				{
					string question = parms?["question"]?.GetValue<string>() ?? string.Empty;
					string button = parms?["buttonText"]?.GetValue<string>() ?? "OK";
					string option = parms?["optionText"]?.GetValue<string>();
					int result = context.AskQuestion?.Invoke(question, button, option) ?? 0;
					return new { result };
				}
				case "host.showComicInfo":
				{
					var ids = parms?["bookIds"]?.AsArray()?.Select(n => n?.GetValue<string>()).Where(s => !string.IsNullOrEmpty(s)).ToList()
						?? new List<string>();
					IEnumerable<ComicBook> books = (context.GetLibraryBooks?.Invoke() ?? Enumerable.Empty<ComicBook>())
						.Where(b => ids.Contains(b.Id.ToString(), StringComparer.OrdinalIgnoreCase));
					context.ShowComicInfo?.Invoke(books);
					return new { ok = true };
				}
				case "host.config.get":
					return new { config = context.LoadConfig?.Invoke() ?? string.Empty };
				case "host.config.set":
				{
					string config = parms?["config"]?.GetValue<string>() ?? string.Empty;
					bool ok = context.SaveConfig?.Invoke(config) ?? false;
					if (!ok)
					{
						throw new HostJsonRpcException(-32000, "Failed to save config");
					}
					return new { ok = true };
				}
				case "host.ui.reload":
					context.ReloadUi?.Invoke();
					return new { ok = true };
				case "host.ui.close":
					context.CloseUi?.Invoke(parms?["dialogResult"]?.GetValue<string>() ?? "ok");
					return new { ok = true };
				default:
					throw new HostJsonRpcException(-32601, "Method not found: " + method);
			}
		}

		private static int Limit(JsonNode parms)
		{
			try
			{
				return parms?["limit"]?.GetValue<int>() ?? 50;
			}
			catch
			{
				return 50;
			}
		}

		private static object[] MapBooks(IEnumerable<ComicBook> books, int limit)
		{
			if (books == null)
			{
				return Array.Empty<object>();
			}
			return books.Take(Math.Max(0, limit)).Select(b => (object)new
			{
				id = b.Id.ToString(),
				caption = b.Caption ?? string.Empty,
				series = b.ShadowSeries ?? string.Empty,
				number = b.ShadowNumber ?? string.Empty,
				volume = (int?)(b.ShadowVolume > 0 ? b.ShadowVolume : (int?)null),
				year = (int?)(b.ShadowYear > 0 ? b.ShadowYear : (int?)null),
				filePath = b.FilePath ?? string.Empty
			}).ToArray();
		}

		private static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

		private static string Success(JsonNode id, object result)
		{
			var payload = new JsonObject
			{
				["jsonrpc"] = "2.0",
				["id"] = id == null ? null : JsonNode.Parse(id.ToJsonString()),
				["result"] = JsonSerializer.SerializeToNode(result, JsonOptions)
			};
			return payload.ToJsonString();
		}

		private static string Error(JsonNode id, int code, string message)
		{
			var payload = new JsonObject
			{
				["jsonrpc"] = "2.0",
				["id"] = id == null ? null : JsonNode.Parse(id.ToJsonString()),
				["error"] = new JsonObject
				{
					["code"] = code,
					["message"] = message
				}
			};
			return payload.ToJsonString();
		}
	}

	public sealed class HostJsonRpcException : Exception
	{
		public int Code { get; }

		public HostJsonRpcException(int code, string message) : base(message)
		{
			Code = code;
		}
	}
}
