using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IAX.IXApi.Bootstrap.Extensions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;

namespace IAX.IXApi.Bootstrap.OpenApi;

/// <summary>Offline discovery only. Never starts the host or resolves a controller/service.</summary>
public static class McpContractExporter
{
    public static WebApplication CreateDiscoveryApplication()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [],
            ApplicationName = typeof(McpContractExporter).Assembly.GetName().Name,
            EnvironmentName = "ContractExport"
        });
        // No appsettings, user-secrets, environment credentials, EF, module registrations,
        // hosted workers, authentication handlers or database initialization are needed.
        builder.Configuration.Sources.Clear();
        builder.Logging.ClearProviders();
        builder.Services.AddApiServices();
        var mvc = builder.Services.AddControllers();
        foreach (var reference in typeof(McpContractExporter).Assembly.GetReferencedAssemblies()
                     .Where(reference => reference.Name!.StartsWith("IAX.IXApi.Modules.", StringComparison.Ordinal)))
            mvc.AddApplicationPart(Assembly.Load(reference));

        builder.Services.Configure<OpenApiOptions>("v1", (OpenApiOptions options) =>
            options.AddOperationTransformer((operation, context, _) =>
            {
                var metadata = McpOperationInventory.Describe(context.Description);
                operation.OperationId = metadata.OperationId;
                operation.Extensions["x-ix-module"] = new OpenApiString(metadata.Module);
                operation.Extensions["x-ix-mcp-exposure"] = new OpenApiString("disabled");
                operation.Extensions["x-ix-discovery-status"] = new OpenApiString(metadata.Status);
                operation.Extensions["x-ix-discovery-reason"] = new OpenApiString(metadata.Reason);
                if (metadata.PilotContract != null)
                    operation.Extensions["x-ix-pilot-contract"] = ToOpenApiValue(JsonSerializer.SerializeToElement(
                        metadata.PilotContract, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                return Task.CompletedTask;
            }));
        var app = builder.Build();
        app.MapControllers();
        app.MapOpenApi();
        return app;
    }

    private static IOpenApiAny ToOpenApiValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var result = new OpenApiObject();
            foreach (var property in element.EnumerateObject())
                result[property.Name] = ToOpenApiValue(property.Value);
            return result;
        }
        if (element.ValueKind == JsonValueKind.Array)
        {
            var result = new OpenApiArray();
            foreach (var item in element.EnumerateArray()) result.Add(ToOpenApiValue(item));
            return result;
        }
        return element.ValueKind switch
        {
            JsonValueKind.String => new OpenApiString(element.GetString()),
            JsonValueKind.Number => new OpenApiInteger(element.GetInt32()),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            JsonValueKind.Null => new OpenApiNull(),
            _ => throw new InvalidOperationException("Unsupported discovery metadata value.")
        };
    }

    public static async Task<ContractExport> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await using var app = CreateDiscoveryApplication();
        var descriptions = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(group => group.Items).ToArray();
        var operations = descriptions.Select(McpOperationInventory.Describe)
            .OrderBy(operation => operation.Path, StringComparer.Ordinal)
            .ThenBy(operation => operation.Method, StringComparer.Ordinal).ToArray();
        if (operations.Length == 0)
            throw new InvalidOperationException("MVC discovery returned no API operations.");
        var duplicates = operations.GroupBy(operation => operation.OperationId).Where(group => group.Count() > 1).ToArray();
        if (duplicates.Length != 0)
            throw new InvalidOperationException("Duplicate generated operations: " + string.Join("; ", duplicates.Select(group =>
                $"{group.First().Method} {group.First().Path} ({group.Count()} descriptions, {group.First().Controller}.{group.First().Action})")));

        // Invoke only the SDK's document endpoint in memory. No StartAsync/RunAsync,
        // listener, database connection, controller activation or business action occurs.
        var endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Single(endpoint => endpoint.RoutePattern.RawText == "/openapi/{documentName}.json");
        await using var scope = app.Services.CreateAsyncScope();
        using var body = new MemoryStream();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Method = "GET";
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("contract-export.invalid");
        context.Request.Path = "/openapi/v1.json";
        context.Request.RouteValues["documentName"] = "v1";
        context.RequestAborted = cancellationToken;
        context.Response.Body = body;
        await endpoint.RequestDelegate!(context);
        if (context.Response.StatusCode != StatusCodes.Status200OK)
            throw new InvalidOperationException($"OpenAPI generation returned {context.Response.StatusCode}.");
        var openApi = Encoding.UTF8.GetString(body.ToArray());
        using var document = JsonDocument.Parse(openApi);
        var documentedCount = document.RootElement.GetProperty("paths").EnumerateObject()
            .Sum(path => path.Value.EnumerateObject().Count(property =>
                property.Name is "get" or "post" or "put" or "patch" or "delete" or "head" or "options" or "trace"));
        if (documentedCount != operations.Length)
            throw new InvalidOperationException($"OpenAPI/MVC count mismatch: {documentedCount}/{operations.Length}.");
        return new ContractExport(openApi, operations);
    }

    public static async Task ExportAsync(string outputDirectory, CancellationToken cancellationToken = default)
    {
        var result = await GenerateAsync(cancellationToken);
        // Generate and validate everything before creating output files.
        var output = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(output);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        await File.WriteAllTextAsync(Path.Combine(output, "ixapi.openapi.json"), result.OpenApi, cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(output, "operations.json"),
            JsonSerializer.Serialize(result.Operations, options), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(output, "pilot-contracts.json"),
            JsonSerializer.Serialize(result.Operations.Where(operation => operation.PilotContract != null)
                .Select(operation => operation.PilotContract), options), cancellationToken);
        var manifest = new
        {
            formatVersion = 1,
            purpose = "discovery-only",
            executable = false,
            apiAssemblyVersion = typeof(McpContractExporter).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            schemaSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.OpenApi))).ToLowerInvariant(),
            operationCount = result.Operations.Length,
            pilotCandidateCount = result.Operations.Count(operation => operation.Status == "pilot-candidate"),
            modules = result.Operations.GroupBy(operation => operation.Module)
                .OrderBy(group => group.Key).ToDictionary(group => group.Key, group => group.Count()),
            note = "Not a production compatibility manifest. No operations are admitted for execution."
        };
        await File.WriteAllTextAsync(Path.Combine(output, "manifest.json"),
            JsonSerializer.Serialize(manifest, options), cancellationToken);
        Console.WriteLine($"Exported {result.Operations.Length} operations to {output}. Exposure remains disabled.");
    }
}

public sealed record ContractExport(string OpenApi, McpOperationDescription[] Operations);
