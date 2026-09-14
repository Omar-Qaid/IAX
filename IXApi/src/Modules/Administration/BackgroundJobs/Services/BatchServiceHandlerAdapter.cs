using System.Text.Json;
using IAX.IXApi.Shared.Application.Batch;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

/// <summary>Compatibility boundary for persisted jobs using the original handler contract.</summary>
internal sealed class BatchServiceHandlerAdapter(string serviceKey, IBatchService service) : ISysBackgroundJobHandler
{
    public string JobKey => serviceKey;
    public async Task ExecuteAsync(SysBackgroundJobContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await service.ProcessAsync(new BatchExecutionContext
        {
            BatchJobId = context.JobId, BatchJobTaskId = context.TaskId,
            ExecutionId = context.CorrelationId, ParametersJson = context.PayloadJson
        }, cancellationToken);
        context.Output = JsonSerializer.Serialize(result);
        if (result.FailedCount > 0)
            throw new InvalidOperationException($"Batch service reported {result.FailedCount} failed item(s): {result.Message}");
    }
}
