using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IAX.IXMcp.Catalog;

namespace IAX.IXMcp.Tests;

public sealed class HostFoundationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private readonly WebApplicationFactory<Program> configuredFactory;

    public HostFoundationTests(WebApplicationFactory<Program> factory)
    {
        configuredFactory = factory
            .WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.ClearProviders()));
        client = configuredFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Liveness_is_healthy()
    {
        using var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Disabled_candidate_catalog_is_not_ready()
    {
        using var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task MCP_HTTP_transport_requires_authentication()
    {
        using var response = await client.GetAsync("/mcp");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
    }

    [Fact]
    public async Task MCP_HTTP_transport_rejects_unapproved_browser_origin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/mcp");
        request.Headers.Add("Origin", "https://attacker.example");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MCP_HTTP_transport_rejects_declared_oversized_body_before_authentication()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new ByteArrayContent(new byte[524289])
        };

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public void C1_candidates_are_loaded_but_not_published()
    {
        var catalog = configuredFactory.Services.GetRequiredService<IMcpCatalogState>();

        Assert.Equal(3, catalog.CandidateToolCount);
        Assert.Equal(0, catalog.PublishedToolCount);
        Assert.False(catalog.IsReady);
    }

    [Fact]
    public void Server_has_no_IXApi_business_assembly_reference()
    {
        var references = typeof(Program).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.DoesNotContain(references, name => name!.StartsWith("IAX.IXApi", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name!.Contains("EntityFrameworkCore", StringComparison.Ordinal));
    }
}
