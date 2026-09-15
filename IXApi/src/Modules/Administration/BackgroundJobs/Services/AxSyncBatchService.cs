using System.Text.Json;
using IAX.IXApi.Shared.Application.Batch;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services;

/// <summary>
/// Example integration batch service. A real AX connector can replace
/// <see cref="IAxSyncProcessor"/> without changing the batch framework.
/// </summary>
public sealed class AxSyncBatchService(IAxSyncProcessor processor) : IBatchService
{
    public Task<BatchExecutionResult> ProcessAsync(
        BatchExecutionContext context,
        CancellationToken cancellationToken)
    {
        AxSyncParameters parameters;
        try
        {
            parameters = string.IsNullOrWhiteSpace(context.ParametersJson)
                ? new AxSyncParameters()
                : JsonSerializer.Deserialize<AxSyncParameters>(context.ParametersJson,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new AxSyncParameters();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("AX sync parameters are not valid JSON.", exception);
        }

        return processor.ProcessAsync(parameters, cancellationToken);
    }
}

public sealed record AxSyncParameters(string? Entity = null, DateTimeOffset? ModifiedSince = null);

/// <summary>Application-specific AX/D365 connector boundary used by the example batch service.</summary>
public interface IAxSyncProcessor
{
    Task<BatchExecutionResult> ProcessAsync(AxSyncParameters parameters, CancellationToken cancellationToken);
}

/// <summary>Safe default used until an AX transport is configured. It performs no external writes.</summary>
internal sealed class UnconfiguredAxSyncProcessor : IAxSyncProcessor
{
    public Task<BatchExecutionResult> ProcessAsync(
        AxSyncParameters parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new InvalidOperationException(
            "AX sync connector is not configured. Register an IAxSyncProcessor implementation before enabling this service.");
    }
}
