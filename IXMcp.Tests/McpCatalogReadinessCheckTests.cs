using IAX.IXMcp.Catalog;
using IAX.IXMcp.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IAX.IXMcp.Tests;

public sealed class McpCatalogReadinessCheckTests
{
    [Fact]
    public async Task Empty_catalog_is_unhealthy()
    {
        var check = new McpCatalogReadinessCheck(new McpCatalogState());

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Equal(0, result.Data["publishedToolCount"]);
    }
}
