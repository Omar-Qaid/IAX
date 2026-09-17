using IAX.IXMcp.Catalog;
using IAX.IXMcp.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Tests;

public sealed class McpCatalogRefreshTests
{
    [Fact]
    public async Task Refresh_replaces_catalog_atomically_and_emergency_deny_retires_known_tool()
    {
        var denied = new[] { string.Empty };
        var state = new McpCatalogState();
        var loader = Loader(state, denied);

        await loader.LoadOnceAsync();
        var first = state.Snapshot;
        Assert.Equal(3, first.Tools.Count(tool => tool.ExecutionEnabled));

        denied[0] = "workflow_requests_get";
        await loader.LoadOnceAsync();
        var second = state.Snapshot;

        Assert.NotSame(first, second);
        Assert.Equal(2, second.Tools.Count(tool => tool.ExecutionEnabled));
        Assert.False(second.Tools.Single(tool => tool.Name == "workflow_requests_get").ExecutionEnabled);
    }

    [Fact]
    public async Task Invalid_refresh_can_retain_previous_snapshot_with_diagnostic()
    {
        var state = new McpCatalogState();
        var loader = Loader(state, []);
        await loader.LoadOnceAsync();
        var accepted = state.Snapshot;

        state.RecordRefreshFailure("fixture refresh rejected");

        Assert.Same(accepted, state.Snapshot);
        Assert.Equal("fixture refresh rejected", state.RefreshError);
        Assert.False(state.IsReady);

        await loader.LoadOnceAsync();
        Assert.Null(state.RefreshError);
        Assert.True(state.IsReady);
    }

    private static McpCatalogLoader Loader(McpCatalogState state, string[] denied)
    {
        var root = FindRepositoryRoot();
        var settings = Options.Create(new IXMcpOptions
        {
            ContractDirectory = Path.Combine(root, ".artifacts", "mcp-contract", "export"),
            IXApiBaseUrl = "https://ixapi.test",
            EnabledReadTools =
            [
                "finance_customers_search",
                "organization_departments_search",
                "workflow_requests_get"
            ],
            EmergencyDeniedTools = denied
        });
        return new McpCatalogLoader(
            new McpCatalogCompiler(),
            state,
            settings,
            new TestEnvironment(root),
            NullLogger<McpCatalogLoader>.Instance);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }

    private sealed class TestEnvironment(string root) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "IXMcp.Tests";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
