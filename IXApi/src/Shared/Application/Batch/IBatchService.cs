namespace IAX.IXApi.Shared.Application.Batch;

public interface IBatchService
{
    Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken);
}

public sealed class BatchExecutionContext
{
    public long BatchJobId { get; init; }
    public long BatchJobTaskId { get; init; }
    public Guid ExecutionId { get; init; }
    public string? ParametersJson { get; init; }
}

public sealed class BatchExecutionResult
{
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public string? Message { get; set; }
}
