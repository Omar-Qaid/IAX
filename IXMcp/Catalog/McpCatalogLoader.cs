using IAX.IXMcp.Configuration;
using Microsoft.Extensions.Options;

namespace IAX.IXMcp.Catalog;

public sealed class McpCatalogLoader(
    McpCatalogCompiler compiler,
    McpCatalogState state,
    IOptions<IXMcpOptions> options,
    IHostEnvironment environment,
    ILogger<McpCatalogLoader> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var configuredPath = options.Value.ContractDirectory;
        var contractDirectory = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(configuredPath, environment.ContentRootPath);

        var catalog = await compiler.CompileAsync(contractDirectory, cancellationToken);
        state.Replace(catalog);
        logger.LogInformation(
            "Loaded {CandidateCount} validated MCP candidates from schema {SchemaSha256}; {ExecutableCount} are executable.",
            catalog.Tools.Count,
            catalog.SchemaSha256,
            catalog.Tools.Count(tool => tool.ExecutionEnabled));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
