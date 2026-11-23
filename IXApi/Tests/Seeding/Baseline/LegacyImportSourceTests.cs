using System.Text.Json;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Execution;
using IAX.IXApi.Infrastructure.Persistence.SeedingERPV2.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace IAX.IXApi.Tests;

/// <summary>The one-time imports read OthersDB when configured, the embedded export only when allowed, and nothing otherwise.</summary>
public sealed class LegacyImportSourceTests
{
    [Fact]
    public async Task Without_a_source_connection_the_embedded_exports_are_read_when_fixtures_are_enabled()
    {
        var source = Source(embedded: true);
        var employees = await source.ReadAsync<JsonElement>("unused", "legacy-employees.json", default);
        Assert.True(employees.GetProperty("Employees").GetArrayLength() > 0);
        Assert.True(employees.GetProperty("Departments").GetArrayLength() > 0);
        var workflow = await source.ReadAsync<JsonElement>("unused", "legacy-workflow-master.json", default);
        Assert.InRange(workflow.GetProperty("Performers").GetArrayLength(), 1, 10);
    }

    [Fact]
    public async Task Without_a_source_connection_or_fixtures_the_import_refuses_to_run()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Source(embedded: false).ReadAsync<JsonElement>("unused", "legacy-employees.json", default));
        Assert.Contains("EnableEmbeddedFixtures", error.Message);
    }

    private static LegacyImportSource Source(bool embedded) =>
        new(new ConfigurationBuilder().Build(), Options.Create(new SeedingOptions { EnableEmbeddedFixtures = embedded }));
}
