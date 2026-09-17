using IAX.IXMcp.Catalog;
using IAX.IXMcp.Configuration;
using IAX.IXMcp.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<IXMcpOptions>()
    .Bind(builder.Configuration.GetSection(IXMcpOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<McpCatalogState>();
builder.Services.AddSingleton<IMcpCatalogState>(services => services.GetRequiredService<McpCatalogState>());
builder.Services.AddSingleton<McpCatalogCompiler>();
builder.Services.AddHostedService<McpCatalogLoader>();
builder.Services
    .AddHealthChecks()
    .AddCheck<McpCatalogReadinessCheck>("mcp_catalog", tags: ["ready"]);

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "IAX.IXMcp",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"
        };
    })
    .WithHttpTransport();

var app = builder.Build();

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
