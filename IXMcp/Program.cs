using IAX.IXMcp.Catalog;
using IAX.IXMcp.Configuration;
using IAX.IXMcp.Health;
using IAX.IXMcp.Execution;
using System.Net;
using IAX.IXMcp.Security;
using IAX.IXMcp.Protocol;
using IAX.IXMcp.Observability;

var builder = WebApplication.CreateBuilder(args);

var requestBodyLimit = builder.Configuration
    .GetSection(IXMcpOptions.SectionName)
    .GetValue<int?>(nameof(IXMcpOptions.MaximumRequestBodyBytes)) ?? 512 * 1024;
builder.WebHost.ConfigureKestrel(serverOptions =>
    serverOptions.Limits.MaxRequestBodySize = requestBodyLimit);

builder.Services
    .AddOptions<IXMcpOptions>()
    .Bind(builder.Configuration.GetSection(IXMcpOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => Uri.TryCreate(options.IXApiBaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.IsLoopback)
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment),
        "IXApiBaseUrl must be an absolute HTTPS origin, except HTTP loopback is allowed for local development.")
    .Validate(options => options.ApprovedPathPrefix.StartsWith("/api/", StringComparison.Ordinal)
            && !options.ApprovedPathPrefix.Contains("..", StringComparison.Ordinal)
            && !options.ApprovedPathPrefix.Contains('\\')
            && !options.ApprovedPathPrefix.Contains('?')
            && !options.ApprovedPathPrefix.Contains('#'),
        "ApprovedPathPrefix must be rooted under /api/ and cannot contain traversal segments.")
    .Validate(options => options.AllowedOrigins.All(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.IsLoopback)
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.PathAndQuery.Trim('/'))
            && string.IsNullOrEmpty(uri.Fragment)),
        "AllowedOrigins must contain absolute HTTPS origins, except HTTP loopback is allowed for local development.")
    .Validate(options => options.MaximumRequestBodyBytes > options.MaximumInputBytes,
        "MaximumRequestBodyBytes must exceed MaximumInputBytes to allow for the MCP JSON-RPC envelope.")
    .ValidateOnStart();

builder.Services.AddSingleton<McpCatalogState>();
builder.Services.AddSingleton<IMcpCatalogState>(services => services.GetRequiredService<McpCatalogState>());
builder.Services.AddSingleton<McpCatalogCompiler>();
builder.Services.AddSingleton<McpCatalogLoader>();
builder.Services.AddHostedService(services => services.GetRequiredService<McpCatalogLoader>());
builder.Services.AddHostedService<McpCatalogRefreshService>();
builder.Services.AddSingleton<ToolArgumentValidator>();
builder.Services.AddSingleton<McpTelemetry>();
builder.Services.AddScoped<McpToolExecutor>();
builder.Services.AddScoped<IMcpToolExecutor, InstrumentedMcpToolExecutor>();
builder.Services.AddScoped<IMcpSessionValidator, IXApiSessionValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<McpProtocolAdapter>();
builder.Services.AddSingleton<PerUserConcurrencyLimiter>();
builder.Services
    .AddHttpClient<IXApiHttpClient>((services, client) =>
    {
        var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<IXMcpOptions>>().Value;
        client.BaseAddress = new Uri(options.IXApiBaseUrl, UriKind.Absolute);
        client.Timeout = Timeout.InfiniteTimeSpan;
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    });
builder.Services
    .AddHealthChecks()
    .AddCheck<McpCatalogReadinessCheck>("mcp_catalog", tags: ["ready"]);

var mcpServer = builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "IAX.IXMcp",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
        };
    })
    .WithHttpTransport();

mcpServer.WithListToolsHandler((request, cancellationToken) =>
{
    var adapter = request.Services!.GetRequiredService<McpProtocolAdapter>();
    return ValueTask.FromResult(adapter.ListTools());
});
mcpServer.WithCallToolHandler(async (request, cancellationToken) =>
{
    var adapter = request.Services!.GetRequiredService<McpProtocolAdapter>();
    return await adapter.CallToolAsync(request.Params!, cancellationToken);
});

var app = builder.Build();

app.UseMiddleware<McpAuthenticationMiddleware>();

app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new()
{
    Predicate = registration => registration.Tags.Contains("ready")
});
app.MapMcp("/mcp");

app.Run();

public partial class Program;
