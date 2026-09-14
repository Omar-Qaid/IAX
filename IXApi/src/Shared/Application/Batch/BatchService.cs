namespace IAX.IXApi.Shared.Application.Batch;

/// <summary>Optional base for services whose business entry point is ProcessAsync.</summary>
public abstract class BatchService : IBatchService
{
    public abstract Task<BatchExecutionResult> ProcessAsync(BatchExecutionContext context, CancellationToken cancellationToken);
}
