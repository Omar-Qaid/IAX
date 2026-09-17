using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Execution;
using IAX.IXMcp.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModelContextProtocol.Client;

namespace IAX.IXMcp.Tests;

public sealed class McpWireIntegrationTests
{
    [Fact]
    public async Task Official_client_discovers_and_calls_admitted_workflow_tool()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["IXMcp:EnabledReadTools:0"] = "workflow_requests_get"
                }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IMcpSessionValidator>();
                services.RemoveAll<IMcpToolExecutor>();
                services.AddScoped<IMcpSessionValidator, AcceptingValidator>();
                services.AddScoped<IMcpToolExecutor, SuccessfulExecutor>();
            });
        });

        using var httpClient = factory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        httpClient.DefaultRequestHeaders.Add("X-Company", "DAT");
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
            Name = "IXMcp integration test"
        }, httpClient, ownsHttpClient: false);
        await using var client = await McpClient.CreateAsync(transport);

        var tools = await client.ListToolsAsync();
        var tool = Assert.Single(tools);
        Assert.Equal("workflow_requests_get", tool.Name);

        var result = await client.CallToolAsync(
            tool.Name,
            new Dictionary<string, object?>
            {
                ["path"] = new Dictionary<string, object?> { ["id"] = "1" }
            });

        Assert.False(result.IsError);
        var structured = result.StructuredContent ?? throw new Xunit.Sdk.XunitException("Structured content is required.");
        Assert.True(structured.GetProperty("ok").GetBoolean());
    }

    private sealed class AcceptingValidator : IMcpSessionValidator
    {
        public Task<SessionValidationResult> ValidateAsync(
            string accessToken,
            string company,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SessionValidationResult.Success(new McpSessionContext(
                "user-a", "alice", accessToken, company, ["User"], ["*"])));
    }

    private sealed class SuccessfulExecutor : IMcpToolExecutor
    {
        public Task<ToolExecutionResult> ExecuteAsync(
            string toolName,
            JsonElement arguments,
            ToolExecutionContext context,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(ToolExecutionResult.Success(new JsonObject
            {
                ["ok"] = true,
                ["data"] = new JsonObject { ["recId"] = 1 },
                ["meta"] = new JsonObject { ["company"] = context.Company }
            }));
    }
}
