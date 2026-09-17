using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Catalog;
using IAX.IXMcp.Execution;
using IAX.IXMcp.Security;
using ModelContextProtocol.Protocol;

namespace IAX.IXMcp.Protocol;

public sealed class McpProtocolAdapter(
    McpCatalogState catalogState,
    IMcpToolExecutor executor,
    IHttpContextAccessor httpContextAccessor)
{
    public ListToolsResult ListTools()
    {
        var session = RequiredSession();
        var tools = catalogState.Snapshot.Tools
            .Where(tool => tool.ExecutionEnabled && HasPermissions(session, tool))
            .Select(ToProtocolTool)
            .ToList();

        return new ListToolsResult { Tools = tools };
    }

    public async ValueTask<CallToolResult> CallToolAsync(
        CallToolRequestParams request,
        CancellationToken cancellationToken)
    {
        var session = RequiredSession();
        var tool = catalogState.Snapshot.Tools.SingleOrDefault(candidate => candidate.Name == request.Name);
        if (tool is null || !tool.ExecutionEnabled || !HasPermissions(session, tool))
        {
            return Error("tool_unavailable", "The requested tool is not available for this session.");
        }

        var arguments = JsonSerializer.SerializeToElement(
            request.Arguments ?? new Dictionary<string, JsonElement>());
        var result = await executor.ExecuteAsync(
            request.Name,
            arguments,
            new ToolExecutionContext(session.AccessToken, session.Company),
            cancellationToken);

        var structured = JsonSerializer.SerializeToElement(result.StructuredContent);
        return new CallToolResult
        {
            IsError = result.IsError,
            StructuredContent = structured,
            Content = [new TextContentBlock { Text = result.StructuredContent.ToJsonString() }]
        };
    }

    private McpSessionContext RequiredSession() =>
        httpContextAccessor.HttpContext?.Items[McpAuthenticationMiddleware.ContextItemKey] as McpSessionContext
        ?? throw new InvalidOperationException("An authenticated MCP session context is required.");

    private static bool HasPermissions(McpSessionContext session, CompiledTool tool)
    {
        var permissions = session.Permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return permissions.Contains("*")
            || tool.RequiredPermissions.All(permissions.Contains);
    }

    private static Tool ToProtocolTool(CompiledTool tool) => new()
    {
        Name = tool.Name,
        Description = $"Read {tool.Module} data through IXApi. Company context and IXApi authorization are enforced.",
        InputSchema = JsonSerializer.SerializeToElement(tool.InputSchema),
        OutputSchema = JsonSerializer.SerializeToElement(BuildOutputSchema(tool))
    };

    private static JsonObject BuildOutputSchema(CompiledTool tool) => new()
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["ok"] = new JsonObject { ["const"] = true },
            ["data"] = tool.OutputDataSchema.DeepClone(),
            ["pagination"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["pageNumber"] = new JsonObject { ["type"] = "integer" },
                    ["pageSize"] = new JsonObject { ["type"] = "integer" },
                    ["totalRecords"] = new JsonObject { ["type"] = "integer" },
                    ["totalPages"] = new JsonObject { ["type"] = "integer" }
                },
                ["additionalProperties"] = false
            },
            ["meta"] = new JsonObject { ["type"] = "object" }
        },
        ["required"] = new JsonArray("ok", "data", "meta"),
        ["additionalProperties"] = false
    };

    private static CallToolResult Error(string code, string message)
    {
        var content = new JsonObject
        {
            ["ok"] = false,
            ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
        };
        return new CallToolResult
        {
            IsError = true,
            StructuredContent = JsonSerializer.SerializeToElement(content),
            Content = [new TextContentBlock { Text = content.ToJsonString() }]
        };
    }
}
