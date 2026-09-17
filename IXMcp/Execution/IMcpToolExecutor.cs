using System.Text.Json;

namespace IAX.IXMcp.Execution;

public interface IMcpToolExecutor
{
    Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        JsonElement arguments,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default);
}
