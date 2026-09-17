using System.Text.Json;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Catalog;

public sealed class McpCatalogRefreshService(
    McpCatalogLoader loader,
    McpCatalogState state,
    IOptions<IXMcpOptions> options,
    ILogger<McpCatalogRefreshService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.CatalogRefreshSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await loader.LoadOnceAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is ContractCompilationException or IOException or JsonException)
            {
                state.RecordRefreshFailure(exception.Message);
                logger.LogError(exception, "Catalog refresh rejected; retaining the previous valid snapshot.");
            }
        }
    }
}
