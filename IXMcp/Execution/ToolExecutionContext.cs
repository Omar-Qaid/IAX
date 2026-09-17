namespace IAX.IXMcp.Execution;

public sealed record ToolExecutionContext(
    string AccessToken,
    string Company,
    string? CorrelationId = null);
