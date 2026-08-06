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






//Update the class in  shared module In to match the UOMDataModel.cs .,TaxDataModel.cs ,MasterDataModel.cs  Requirements: All ERP classes must inherit only from entity classes. Do not inherit from any other base classes. Compare every class in UOMDataModel.cs .,TaxDataModel.cs ,MasterDataModel.cs with the corresponding class in the shared module. Ensure all missing classes are added. Update existing classes to have the same structure, naming, properties, relationships, and inheritance as the  UOMDataModel.cs .,TaxDataModel.cs ,MasterDataModel.cs. Make  shared module fully consistent with the UOMDataModel.cs ,TaxDataModel.cs ,MasterDataModel.cs .
//Update the  ledgerManagement module to match ledgerDataModel.cs`  file. Requirements: *All ERP classes must inherit **only** from entity classes. * Do **not** inherit from any other base classes. * Compare every class in ledgerManagement module  with the corresponding class in the ledgerDataModel.cs`  file module. * Ensure all missing classes are added. * Update existing classes to have the same structure, naming, properties, relationships, and inheritance as the ledger Management module. * Make ledger Management module  fully consistent with the  ledgerDataModel.cs`  file .
//Update the class in  AccountsReceivablemodule In to match the CustomerDataModel.cs. Requirements: All ERP classes must inherit only from entity classes. Do not inherit from any other base classes. Compare every class in CustomerDataModel.cs with the corresponding class in the AccountsReceivable module. Ensure all missing classes are added. Update existing classes to have the same structure, naming, properties, relationships, and inheritance as the CustomerDataModel.cs. Make  AccountsReceivable module fully consistent with the CustomerDataModel.cs .
//Update the  inveintoryManagement module to match inveintoryDataModel.cs`  file. Requirements: *All ERP classes must inherit **only** from entity classes. * Do **not** inherit from any other base classes. * Compare every class in inveintoryManagement module  with the corresponding class in the inveintoryDataModel.cs`  file module. * Ensure all missing classes are added. * Update existing classes to have the same structure, naming, properties, relationships, and inheritance as the inveintory Management module. * Make inveintory Management module  fully consistent with the  inveintoryDataModel.cs`  file .
//do not change enum to  int
//check it  BaseEntity<T> 
//ignore upper case  or lower case
