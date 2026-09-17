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
    public Task StartAsync(CancellationToken cancellationToken) => LoadOnceAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task LoadOnceAsync(CancellationToken cancellationToken = default)
    {
        var configuredPath = options.Value.ContractDirectory;
        var contractDirectory = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(configuredPath, environment.ContentRootPath);

        var discovered = await compiler.CompileAsync(contractDirectory, cancellationToken);
        var enabledNames = options.Value.EnabledReadTools.ToHashSet(StringComparer.Ordinal);
        enabledNames.ExceptWith(options.Value.EmergencyDeniedTools);
        var unknownNames = enabledNames.Except(discovered.Tools.Select(tool => tool.Name), StringComparer.Ordinal).ToArray();
        if (unknownNames.Length > 0)
        {
            throw new ContractCompilationException(
                $"EnabledReadTools contains unknown tools: {string.Join(", ", unknownNames)}.");
        }

        var catalog = discovered with
        {
            Tools = discovered.Tools
                .Select(tool => tool with
                {
                    ExecutionEnabled = enabledNames.Contains(tool.Name)
                        && tool.Method.Equals("GET", StringComparison.Ordinal)
                })
                .ToArray()
        };
        state.Replace(catalog);
        logger.LogInformation(
            "Loaded {CandidateCount} validated MCP candidates from schema {SchemaSha256}; {ExecutableCount} are executable.",
            catalog.Tools.Count,
            catalog.SchemaSha256,
            catalog.Tools.Count(tool => tool.ExecutionEnabled));
    }
}
