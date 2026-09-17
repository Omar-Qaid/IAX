using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using IAX.IXMcp.Catalog;

namespace IAX.IXMcp.Tests;

public sealed class McpCatalogCompilerTests
{
    [Fact]
    public async Task Compiles_the_real_C1_contract_into_three_disabled_candidates()
    {
        var compiler = new McpCatalogCompiler();
        var directory = Path.Combine(FindRepositoryRoot(), ".artifacts", "mcp-contract", "export");

        var catalog = await compiler.CompileAsync(directory);

        Assert.Equal(3, catalog.Tools.Count);
        Assert.All(catalog.Tools, tool => Assert.False(tool.ExecutionEnabled));
        Assert.Equal(
            ["finance_customers_search", "organization_departments_search", "workflow_requests_get"],
            catalog.Tools.Select(tool => tool.Name).ToArray());
        var workflow = Assert.Single(catalog.Tools, tool => tool.Name == "workflow_requests_get");
        Assert.Equal("string", workflow.InputSchema["properties"]?["path"]?["properties"]?["id"]?["type"]?.GetValue<string>());
        Assert.Equal(workflow.AllowedDataFields.Count, workflow.OutputDataSchema["properties"]?.AsObject().Count);
        Assert.False(workflow.ResponseDataIsArray);
        Assert.True(catalog.Tools.Single(tool => tool.Name == "finance_customers_search").ResponseDataIsArray);
        Assert.True(catalog.Tools.Single(tool => tool.Name == "organization_departments_search").ResponseDataIsArray);
    }

    [Fact]
    public async Task New_approved_endpoint_updates_catalog_without_adapter_changes()
    {
        using var fixture = ContractFixture.FromCurrentExport();
        fixture.CloneDepartmentAs("organization_teams_search", "/api/v1/Team/paged", "Organization_Team_GetPaged_GET_fixture");
        fixture.Save();

        var catalog = await new McpCatalogCompiler().CompileAsync(fixture.Directory);

        Assert.Equal(4, catalog.Tools.Count);
        Assert.Contains(catalog.Tools, tool => tool.Name == "organization_teams_search");
    }

    [Fact]
    public async Task DTO_field_change_updates_projected_output_without_adapter_changes()
    {
        using var fixture = ContractFixture.FromCurrentExport();
        fixture.AddDepartmentOutputField("externalCode", new JsonObject { ["type"] = "string" });
        fixture.Save();

        var catalog = await new McpCatalogCompiler().CompileAsync(fixture.Directory);

        var department = Assert.Single(catalog.Tools, tool => tool.Name == "organization_departments_search");
        Assert.Equal("string", department.OutputDataSchema["properties"]?["externalCode"]?["type"]?.GetValue<string>());
    }

    [Fact]
    public async Task Corrupted_OpenAPI_hash_is_rejected_clearly()
    {
        using var fixture = ContractFixture.FromCurrentExport();
        fixture.Save();
        await File.AppendAllTextAsync(Path.Combine(fixture.Directory, "ixapi.openapi.json"), " ");

        var exception = await Assert.ThrowsAsync<ContractCompilationException>(
            () => new McpCatalogCompiler().CompileAsync(fixture.Directory));

        Assert.Contains("SHA-256 mismatch", exception.Message);
    }

    [Fact]
    public async Task Drift_between_OpenAPI_and_policy_contract_is_rejected()
    {
        using var fixture = ContractFixture.FromCurrentExport();
        fixture.ChangeEmbeddedDepartmentToolName("organization_wrong_name");
        fixture.Save();

        var exception = await Assert.ThrowsAsync<ContractCompilationException>(
            () => new McpCatalogCompiler().CompileAsync(fixture.Directory));

        Assert.Contains("must be 'organization_departments_search'", exception.Message);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private sealed class ContractFixture : IDisposable
    {
        private readonly JsonObject openApi;
        private readonly JsonArray operations;
        private readonly JsonArray pilots;
        private readonly JsonObject manifest;

        private ContractFixture(
            string directory,
            JsonObject openApi,
            JsonArray operations,
            JsonArray pilots,
            JsonObject manifest)
        {
            Directory = directory;
            this.openApi = openApi;
            this.operations = operations;
            this.pilots = pilots;
            this.manifest = manifest;
        }

        public string Directory { get; }

        public static ContractFixture FromCurrentExport()
        {
            var source = Path.Combine(FindRepositoryRoot(), ".artifacts", "mcp-contract", "export");
            var directory = Path.Combine(Path.GetTempPath(), $"ixmcp-contract-{Guid.NewGuid():N}");
            System.IO.Directory.CreateDirectory(directory);

            return new ContractFixture(
                directory,
                ReadObject(source, "ixapi.openapi.json"),
                ReadArray(source, "operations.json"),
                ReadArray(source, "pilot-contracts.json"),
                ReadObject(source, "manifest.json"));
        }

        public void CloneDepartmentAs(string toolName, string path, string operationId)
        {
            const string sourcePath = "/api/v1/Department/paged";
            var sourceOperation = openApi["paths"]?[sourcePath]?["get"]?.DeepClone().AsObject()
                ?? throw new InvalidOperationException("Department OpenAPI operation not found.");
            sourceOperation["operationId"] = operationId;
            sourceOperation["x-ix-pilot-contract"]!["toolName"] = toolName;
            sourceOperation["x-ix-pilot-contract"]!["path"] = path;
            openApi["paths"]![path] = new JsonObject { ["get"] = sourceOperation };

            var sourceInventory = operations.OfType<JsonObject>()
                .Single(operation => operation["proposedToolName"]?.GetValue<string>() == "organization_departments_search")
                .DeepClone().AsObject();
            sourceInventory["operationId"] = operationId;
            sourceInventory["path"] = path;
            sourceInventory["proposedToolName"] = toolName;
            sourceInventory["pilotContract"]!["toolName"] = toolName;
            sourceInventory["pilotContract"]!["path"] = path;
            operations.Add(sourceInventory);

            var sourcePilot = pilots.OfType<JsonObject>()
                .Single(pilot => pilot["toolName"]?.GetValue<string>() == "organization_departments_search")
                .DeepClone().AsObject();
            sourcePilot["toolName"] = toolName;
            sourcePilot["path"] = path;
            pilots.Add(sourcePilot);
        }

        public void AddDepartmentOutputField(string fieldName, JsonObject schema)
        {
            openApi["components"]!["schemas"]!["DepartmentDto"]!["properties"]![fieldName] = schema;
            pilots.OfType<JsonObject>()
                .Single(pilot => pilot["toolName"]?.GetValue<string>() == "organization_departments_search")
                ["allowedDataFields"]!.AsArray().Add(fieldName);
            var operation = openApi["paths"]?["/api/v1/Department/paged"]?["get"]?[
                "x-ix-pilot-contract"]?.AsObject()
                ?? throw new InvalidOperationException("Embedded Department contract not found.");
            operation["allowedDataFields"]!.AsArray().Add(fieldName);
        }

        public void ChangeEmbeddedDepartmentToolName(string toolName)
        {
            var embedded = openApi["paths"]?["/api/v1/Department/paged"]?["get"]?[
                "x-ix-pilot-contract"]?.AsObject()
                ?? throw new InvalidOperationException("Embedded Department contract not found.");
            embedded["toolName"] = toolName;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var openApiBytes = JsonSerializer.SerializeToUtf8Bytes(openApi, options);
            File.WriteAllBytes(Path.Combine(Directory, "ixapi.openapi.json"), openApiBytes);
            File.WriteAllText(Path.Combine(Directory, "operations.json"), operations.ToJsonString(options));
            File.WriteAllText(Path.Combine(Directory, "pilot-contracts.json"), pilots.ToJsonString(options));

            manifest["schemaSha256"] = Convert.ToHexString(SHA256.HashData(openApiBytes)).ToLowerInvariant();
            manifest["operationCount"] = operations.Count;
            manifest["pilotCandidateCount"] = pilots.Count;
            File.WriteAllText(Path.Combine(Directory, "manifest.json"), manifest.ToJsonString(options));
        }

        public void Dispose()
        {
            System.IO.Directory.Delete(Directory, recursive: true);
        }

        private static JsonObject ReadObject(string source, string name) =>
            JsonNode.Parse(File.ReadAllBytes(Path.Combine(source, name)))?.AsObject()
            ?? throw new InvalidOperationException($"Could not read {name}.");

        private static JsonArray ReadArray(string source, string name) =>
            JsonNode.Parse(File.ReadAllBytes(Path.Combine(source, name)))?.AsArray()
            ?? throw new InvalidOperationException($"Could not read {name}.");
    }
}
