using System.Text.Json;
using IAX.IXApi.Bootstrap.OpenApi;
using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Identity.Permissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class McpContractDiscoveryTests
{
    [Theory]
    [InlineData("GET", "View")]
    [InlineData("post", "Create")]
    [InlineData("PUT", "Edit")]
    [InlineData("PATCH", "Edit")]
    [InlineData("DELETE", "Delete")]
    public void Permission_metadata_uses_the_execution_mapping(string method, string action)
    {
        Assert.Equal($"Module.Resource.{action}",
            new DomainPermissionAttribute("Module", "Resource").GetRequiredPermission(method));
        Assert.Equal("Module.Resource.Run",
            new DomainPermissionAttribute("Module", "Resource", "Run").GetRequiredPermission(method));
    }

    [Fact]
    public async Task Discovery_does_not_register_database_or_business_workers()
    {
        await using var app = McpContractExporter.CreateDiscoveryApplication();
        Assert.Null(app.Services.GetService<ApplicationDbContext>());
        Assert.DoesNotContain(app.Services.GetServices<IHostedService>(), service =>
            service.GetType().Assembly.GetName().Name!.StartsWith("IAX.", StringComparison.Ordinal));
        Assert.Null(app.Configuration["ConnectionStrings:DbConnString"]);
        Assert.Empty(app.Urls);
    }

    [Fact]
    public async Task Export_covers_modules_preserves_exclusions_and_keeps_execution_disabled()
    {
        var result = await McpContractExporter.GenerateAsync();
        foreach (var module in new[] { "Administration", "Communication", "Finance", "Identity", "Organization", "Workflow" })
            Assert.Contains(result.Operations, operation => operation.Module == module);
        Assert.Equal(3, result.Operations.Count(operation => operation.Status == "pilot-candidate"));
        Assert.DoesNotContain(result.Operations, operation => operation.Controller == "WfRequest"
            && operation.Action is "Create" or "GetPaged" or "CreateRange" or "UpdateRange" or "DeleteRange");
        var organizationUnits = Assert.Single(result.Operations, operation =>
            operation.ProposedToolName == "organization_units_list");
        Assert.Contains("Organization.Structure.View", organizationUnits.DomainPermissions);
        var customer = Assert.Single(result.Operations, operation =>
            operation.ProposedToolName == "finance_customers_search");
        Assert.Contains(result.Operations, operation => operation.Path == "/api/v1/CustTable/paged"
            && operation.SameActionGroup == customer.SameActionGroup && operation.Status == "excluded");
        var workflow = Assert.Single(result.Operations, operation =>
            operation.ProposedToolName == "workflow_requests_get");
        Assert.Empty(workflow.DomainPermissions);
        Assert.Contains("CanAccessRequestAsync", workflow.RecordAuthorization);
        Assert.All(result.Operations.Where(operation => operation.Method != "GET"), operation =>
            Assert.Equal("excluded", operation.Status));

        using var document = JsonDocument.Parse(result.OpenApi);
        var paths = document.RootElement.GetProperty("paths");
        foreach (var operation in result.Operations)
        {
            var contract = paths.GetProperty(operation.Path).GetProperty(operation.Method.ToLowerInvariant());
            Assert.Equal(operation.OperationId, contract.GetProperty("operationId").GetString());
            Assert.Equal("disabled", contract.GetProperty("x-ix-mcp-exposure").GetString());
        }
        foreach (var candidate in result.Operations.Where(operation => operation.Status == "pilot-candidate"))
        {
            var response = paths.GetProperty(candidate.Path).GetProperty("get")
                .GetProperty("responses").GetProperty("200").GetProperty("content");
            Assert.Contains(response.EnumerateObject(), media => media.Value.TryGetProperty("schema", out _));
            var policy = Assert.IsType<McpPilotContract>(candidate.PilotContract);
            Assert.False(policy.ExecutionEnabled);
            Assert.Equal("company-required", policy.CompanyScope);
            var schema = Resolve(response.GetProperty("application/json").GetProperty("schema"), document.RootElement);
            var data = schema.TryGetProperty("properties", out var properties)
                && properties.TryGetProperty("data", out var wrappedData)
                    ? Resolve(wrappedData, document.RootElement)
                    : schema;
            if (data.TryGetProperty("items", out var items)) data = Resolve(items, document.RootElement);
            foreach (var field in policy.AllowedDataFields)
                Assert.Contains(data.GetProperty("properties").EnumerateObject(), property => property.Name == field);
            Assert.DoesNotContain("bankAccount", policy.AllowedDataFields);
            Assert.DoesNotContain("createdByUser", policy.AllowedDataFields);
            Assert.DoesNotContain("Includes", policy.AllowedQueryParameters);
            Assert.DoesNotContain("Filters", policy.AllowedQueryParameters);
            foreach (var parameter in policy.AllowedQueryParameters)
                Assert.Contains(candidate.Parameters, entry => entry.Source == "Query" && entry.Name == parameter);
        }
    }

    private static JsonElement Resolve(JsonElement schema, JsonElement document)
    {
        for (var depth = 0; depth < 16 && schema.TryGetProperty("$ref", out var reference); depth++)
        {
            var path = reference.GetString()!;
            Assert.StartsWith("#/", path);
            schema = document;
            foreach (var segment in path[2..].Split('/'))
                schema = schema.GetProperty(segment.Replace("~1", "/").Replace("~0", "~"));
        }
        Assert.False(schema.TryGetProperty("$ref", out _), "Unresolved or cyclic root schema reference.");
        return schema;
    }
}
