using System.Text.Json.Nodes;
using cYo.Projects.ComicRack.Plugins.WebView;
using Xunit;

namespace ComicRack.Tests.Plugins
{
	public class HostJsonRpcTests
	{
		[Fact]
		public void GetInfo_ReturnsApiVersion1()
		{
			var context = new HostJsonRpcContext
			{
				ProductVersion = "0.9.200",
				PluginId = "demo",
				PluginVersion = "1.0.0",
				CommandPath = @"C:\Scripts\demo"
			};
			string response = HostJsonRpc.HandleMessage(
				"""{"jsonrpc":"2.0","id":1,"method":"host.getInfo"}""",
				context);
			JsonNode node = JsonNode.Parse(response);
			Assert.Equal(1, node["id"]!.GetValue<int>());
			Assert.Equal(1, node["result"]!["apiVersion"]!.GetValue<int>());
			Assert.Equal("demo", node["result"]!["pluginId"]!.GetValue<string>());
			Assert.Null(node["error"]);
		}

		[Fact]
		public void UnknownMethod_ReturnsMinus32601()
		{
			string response = HostJsonRpc.HandleMessage(
				"""{"jsonrpc":"2.0","id":"x","method":"host.nope"}""",
				new HostJsonRpcContext());
			JsonNode node = JsonNode.Parse(response);
			Assert.Equal(-32601, node["error"]!["code"]!.GetValue<int>());
		}

		[Fact]
		public void ConfigRoundTrip_ViaContextDelegates()
		{
			string stored = string.Empty;
			var context = new HostJsonRpcContext
			{
				LoadConfig = () => stored,
				SaveConfig = s => { stored = s; return true; }
			};
			string setResponse = HostJsonRpc.HandleMessage(
				"""{"jsonrpc":"2.0","id":2,"method":"host.config.set","params":{"config":"hello"}}""",
				context);
			Assert.Null(JsonNode.Parse(setResponse)!["error"]);
			string getResponse = HostJsonRpc.HandleMessage(
				"""{"jsonrpc":"2.0","id":3,"method":"host.config.get"}""",
				context);
			Assert.Equal("hello", JsonNode.Parse(getResponse)!["result"]!["config"]!.GetValue<string>());
		}
	}
}
