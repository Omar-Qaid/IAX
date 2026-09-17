using IAX.IXMcp.Catalog;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IAX.IXMcp.Health;

public sealed class McpCatalogReadinessCheck(IMcpCatalogState catalog) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!catalog.IsReady || catalog.PublishedToolCount == 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                catalog.Status,
                data: new Dictionary<string, object>
                {
                    ["publishedToolCount"] = catalog.PublishedToolCount,
                    ["candidateToolCount"] = catalog.CandidateToolCount
                }));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            "The validated MCP catalog is ready.",
            new Dictionary<string, object>
            {
                ["publishedToolCount"] = catalog.PublishedToolCount,
                ["candidateToolCount"] = catalog.CandidateToolCount
            }));
    }
}
