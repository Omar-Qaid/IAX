using System.Text.Json.Nodes;

namespace IAX.IXMcp.Execution;

public sealed record ToolExecutionResult(
    bool IsError,
    string Code,
    JsonObject StructuredContent)
{
    public static ToolExecutionResult Success(JsonObject content) => new(false, "ok", content);

    public static ToolExecutionResult Failure(
        string code,
        string message,
        string correlationId,
        int? downstreamStatus = null)
    {
        var meta = new JsonObject
        {
            ["correlationId"] = correlationId
        };
        if (downstreamStatus is not null)
        {
            meta["downstreamStatus"] = downstreamStatus.Value;
        }

        return new ToolExecutionResult(
            true,
            code,
            new JsonObject
            {
                ["ok"] = false,
                ["error"] = new JsonObject
                {
                    ["code"] = code,
                    ["message"] = message
                },
                ["meta"] = meta
            });
    }
}
