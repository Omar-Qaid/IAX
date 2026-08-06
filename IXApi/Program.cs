using IAX.IXApi.Shared.Application.Querying;
using IAX.IXApi.Api.Middleware;
using IAX.IXApi.Infrastructure.Persistence.Seeding;
using IAX.IXApi.Bootstrap.Extensions;
using IAX.IXApi.Bootstrap;
using IAX.IXApi.Modules.Workflow;
using IAX.IXApi.Modules.ERP;
using IAX.IXApi.Modules.Identity;
using IAX.IXApi.Modules.Organization;
using IAX.IXApi.Modules.Communication;
using IAX.IXApi.Modules.Administration;
using Mapster;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;

const string MyAllowSpecificOrigins = "CorsPolicy";
const string RateLimitPolicyTight = "tight";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Increase Kestrel request-line limit so that SignalR WebSocket URLs with large
// JWT access_token query params (containing many permission claims) are not rejected.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestLineSize = 16384;
    options.Limits.MaxRequestHeadersTotalSize = 16384;
});

// 1. Infrastructure & Core Services
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAX.IXApi.Infrastructure.Caching.IErpLookupCacheService, IAX.IXApi.Infrastructure.Caching.ErpLookupCacheService>();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddMapster();

// 2. Modular Domain Registrations
var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddApplicationServices(assembly);
builder.Services.AddCommunicationModule(builder.Configuration);
builder.Services.AddAdministrationModule(builder.Configuration, assembly);
builder.Services.AddWorkflowModule(builder.Configuration);
builder.Services.AddErpModule(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddOrganizationModule(builder.Configuration);

// 2. Security & Auth
builder.Services.AddCustomIdentity();
builder.Services.AddCustomConfigurations(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddCustomAuthorization();

// 3. API & Middleware Services
builder.Services.AddApiServices();
builder.Services.AddDomainServices();
builder.Services.AddCustomRateLimiter();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOutputCache(o =>
{
    o.AddPolicy("default", p => p.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
});

// 4. CORS
string[] corsAllowedOrigins = builder.Configuration
    .GetSection("CorsAllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? [];

if (corsAllowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "At least one trusted origin must be configured in CorsAllowedOrigins.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddTransient<CorrelationIdMiddleware>();

var app = builder.Build();

// 1. Pipeline - Order Matters
app.UseExceptionHandler();
app.UseInfrastructureMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseApiDocumentation();
}
else
{
    app.UseHsts();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();
app.UseResponseCompression();
app.UseRequestDecompression();

// 2. Endpoints
app.MapControllers().RequireRateLimiting(RateLimitPolicyTight);
app.MapHub<IAX.IXApi.Infrastructure.Realtime.SysRealtimeHub>("/hubs/realtime");
app.MapHub<IAX.IXApi.Infrastructure.Realtime.SysChatHub>("/hubs/chat");
app.MapHealthChecks("/health").AllowAnonymous();

// 3. Data Initialization
if (builder.Configuration.GetValue("DatabaseInitialization:Enabled", true))
{
    await app.InitializeDatabaseAsync();
}

app.Run();
