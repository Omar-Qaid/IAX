using System.Diagnostics;
using System.Text.Json;
using IAX.IXMcp.Execution;

namespace IAX.IXMcp.Observability;

public sealed class InstrumentedMcpToolExecutor(
    McpToolExecutor inner,
    McpTelemetry telemetry) : IMcpToolExecutor
{
    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        JsonElement arguments,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var started = Stopwatch.GetTimestamp();
        ToolExecutionResult result;
        try
        {
            result = await inner.ExecuteAsync(toolName, arguments, context, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            telemetry.Calls.Add(1, new("tool", toolName), new("outcome", "cancelled"));
            throw;
        }

        telemetry.Calls.Add(1, new("tool", toolName), new("outcome", result.Code));
        TagList durationTags = new() { { "tool", toolName } };
        telemetry.Duration.Record(Stopwatch.GetElapsedTime(started).TotalMilliseconds, durationTags);
        return result;
    }
}
