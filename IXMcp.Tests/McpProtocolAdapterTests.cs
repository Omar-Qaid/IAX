using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Catalog;
using IAX.IXMcp.Execution;
using IAX.IXMcp.Protocol;
using IAX.IXMcp.Security;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;

namespace IAX.IXMcp.Tests;

public sealed class McpProtocolAdapterTests
{
    [Fact]
    public void Discovery_lists_only_enabled_tools_allowed_for_the_session()
    {
        var state = State(
            Tool("allowed", true, ["Finance.View"]),
            Tool("denied", true, ["Admin.View"]),
            Tool("disabled", false, []));
        var adapter = Adapter(state, new FakeExecutor(), Session(["Finance.View"]));

        var result = adapter.ListTools();

        var tool = Assert.Single(result.Tools);
        Assert.Equal("allowed", tool.Name);
        Assert.Equal("object", tool.InputSchema.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Call_uses_authenticated_session_token_and_company()
    {
        var executor = new FakeExecutor();
        var state = State(Tool("allowed", true, ["Finance.View"]));
        var adapter = Adapter(state, executor, Session(["Finance.View"]));

        var result = await adapter.CallToolAsync(new CallToolRequestParams
        {
            Name = "allowed",
            Arguments = new Dictionary<string, JsonElement>()
        }, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal("token-a", executor.Context?.AccessToken);
        Assert.Equal("DAT", executor.Context?.Company);
    }

    [Fact]
    public async Task Permission_denied_tool_is_not_callable_by_name()
    {
        var executor = new FakeExecutor();
        var state = State(Tool("admin", true, ["Admin.View"]));
        var adapter = Adapter(state, executor, Session(["Finance.View"]));

        var result = await adapter.CallToolAsync(new CallToolRequestParams { Name = "admin" }, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Null(executor.Context);
    }

    private static McpProtocolAdapter Adapter(
        McpCatalogState state,
        IMcpToolExecutor executor,
        McpSessionContext session)
    {
        var context = new DefaultHttpContext();
        context.Items[McpAuthenticationMiddleware.ContextItemKey] = session;
        return new McpProtocolAdapter(state, executor, new HttpContextAccessor { HttpContext = context });
    }

    private static McpSessionContext Session(IReadOnlyList<string> permissions) =>
        new("user-a", "alice", "token-a", "DAT", ["User"], permissions);

    private static McpCatalogState State(params CompiledTool[] tools)
    {
        var state = new McpCatalogState();
        state.Replace(new CompiledCatalog("hash", "release", tools));
        return state;
    }

    private static CompiledTool Tool(string name, bool enabled, IReadOnlyList<string> permissions) => new(
        name, 1, "Test", $"Test_{name}", "GET", "/api/v1/Test", enabled, "company-required",
        new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(),
            ["additionalProperties"] = false
        },
        new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(),
            ["additionalProperties"] = false
        },
        [], [], permissions, "test", []);

    private sealed class FakeExecutor : IMcpToolExecutor
    {
        public ToolExecutionContext? Context { get; private set; }

        public Task<ToolExecutionResult> ExecuteAsync(
            string toolName,
            JsonElement arguments,
            ToolExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            Context = context;
            return Task.FromResult(ToolExecutionResult.Success(new JsonObject
            {
                ["ok"] = true,
                ["data"] = new JsonObject(),
                ["meta"] = new JsonObject()
            }));
        }
    }
}
