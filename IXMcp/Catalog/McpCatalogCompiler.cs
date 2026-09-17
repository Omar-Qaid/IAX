using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IAX.IXMcp.Catalog;

public sealed class McpCatalogCompiler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<CompiledCatalog> CompileAsync(
        string contractDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contractDirectory);

        var manifestPath = RequiredFile(contractDirectory, "manifest.json");
        var openApiPath = RequiredFile(contractDirectory, "ixapi.openapi.json");
        var operationsPath = RequiredFile(contractDirectory, "operations.json");
        var pilotsPath = RequiredFile(contractDirectory, "pilot-contracts.json");

        var manifest = await ReadAsync<DiscoveryManifest>(manifestPath, cancellationToken);
        var operations = await ReadAsync<OperationInventory[]>(operationsPath, cancellationToken);
        var pilots = await ReadAsync<PilotContract[]>(pilotsPath, cancellationToken);
        var openApiBytes = await File.ReadAllBytesAsync(openApiPath, cancellationToken);

        ValidateManifest(manifest, openApiBytes, operations, pilots);

        JsonNode openApi;
        try
        {
            openApi = JsonNode.Parse(openApiBytes)
                ?? throw new ContractCompilationException("ixapi.openapi.json is empty.");
        }
        catch (JsonException exception)
        {
            throw new ContractCompilationException($"ixapi.openapi.json is invalid JSON: {exception.Message}");
        }

        var duplicateTool = pilots
            .GroupBy(contract => contract.ToolName, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateTool is not null)
        {
            throw new ContractCompilationException($"Duplicate tool name '{duplicateTool.Key}'.");
        }

        var compiled = pilots
            .Select(contract => CompileTool(contract, operations, openApi))
            .OrderBy(tool => tool.Name, StringComparer.Ordinal)
            .ToArray();

        return new CompiledCatalog(manifest.SchemaSha256, manifest.ApiAssemblyVersion, compiled);
    }

    private static CompiledTool CompileTool(
        PilotContract contract,
        IReadOnlyList<OperationInventory> operations,
        JsonNode openApi)
    {
        if (contract.ExecutionEnabled)
        {
            throw new ContractCompilationException(
                $"Discovery contract '{contract.ToolName}' cannot enable execution.");
        }

        if (contract.MetadataVersion != 1 || contract.ContractVersion < 1)
        {
            throw new ContractCompilationException(
                $"Tool '{contract.ToolName}' has an unsupported metadata or contract version.");
        }

        var matches = operations
            .Where(operation => string.Equals(operation.Method, contract.Method, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation.Path, contract.Path, StringComparison.Ordinal))
            .ToArray();
        if (matches.Length != 1)
        {
            throw new ContractCompilationException(
                $"Tool '{contract.ToolName}' must match exactly one operation inventory record; found {matches.Length}.");
        }

        var operation = matches[0];
        if (operation.Status != "pilot-candidate"
            || operation.AllowsAnonymous
            || !string.Equals(operation.ProposedToolName, contract.ToolName, StringComparison.Ordinal))
        {
            throw new ContractCompilationException(
                $"Operation '{operation.OperationId}' is not a secured pilot candidate for '{contract.ToolName}'.");
        }

        var openApiOperation = FindOpenApiOperation(openApi, contract.Path, contract.Method, contract.ToolName);
        RequireString(openApiOperation, "operationId", operation.OperationId, contract.ToolName);
        RequireString(openApiOperation, "x-ix-module", operation.Module, contract.ToolName);
        RequireString(openApiOperation, "x-ix-mcp-exposure", "disabled", contract.ToolName);
        RequireString(openApiOperation, "x-ix-discovery-status", "pilot-candidate", contract.ToolName);
        ValidateEmbeddedPilotContract(openApiOperation, contract);

        var parameters = ReadParameters(openApiOperation, contract);
        var inputSchema = BuildInputSchema(parameters);
        var outputSchema = BuildOutputSchema(openApi, openApiOperation, contract);

        return new CompiledTool(
            contract.ToolName,
            contract.ContractVersion,
            operation.Module,
            operation.OperationId,
            contract.Method.ToUpperInvariant(),
            contract.Path,
            false,
            contract.CompanyScope,
            inputSchema,
            outputSchema,
            parameters.Select(parameter => new BindingParameter(
                parameter.Name,
                parameter.Location,
                parameter.Required)).ToArray(),
            contract.AllowedDataFields.ToArray(),
            operation.DomainPermissions.ToArray(),
            contract.RecordAuthorization,
            contract.RequiredEvidence.ToArray());
    }

    private static IReadOnlyList<ParameterDefinition> ReadParameters(JsonObject operation, PilotContract contract)
    {
        var documented = (operation["parameters"] as JsonArray ?? [])
            .OfType<JsonObject>()
            .Select(parameter => new ParameterDefinition(
                RequiredText(parameter, "name", contract.ToolName),
                RequiredText(parameter, "in", contract.ToolName),
                parameter["required"]?.GetValue<bool>() ?? false,
                (parameter["schema"] as JsonObject)?.DeepClone().AsObject()
                    ?? throw new ContractCompilationException(
                        $"Parameter in '{contract.ToolName}' is missing its schema.")))
            .ToArray();

        var pathParameters = documented
            .Where(parameter => parameter.Location == "path")
            .ToArray();
        foreach (var placeholder in PathPlaceholders(contract.Path))
        {
            if (!pathParameters.Any(parameter => parameter.Name == placeholder && parameter.Required))
            {
                throw new ContractCompilationException(
                    $"Path parameter '{placeholder}' for '{contract.ToolName}' is missing or optional.");
            }
        }

        var allowedQueryNames = contract.AllowedQueryParameters.ToHashSet(StringComparer.Ordinal);
        foreach (var allowedName in allowedQueryNames)
        {
            if (!documented.Any(parameter => parameter.Location == "query" && parameter.Name == allowedName))
            {
                throw new ContractCompilationException(
                    $"Allowed query parameter '{allowedName}' is not documented for '{contract.ToolName}'.");
            }
        }

        return documented
            .Where(parameter => parameter.Location == "path"
                || parameter.Location == "query" && allowedQueryNames.Contains(parameter.Name))
            .ToArray();
    }

    private static JsonObject BuildInputSchema(IReadOnlyList<ParameterDefinition> parameters)
    {
        var rootProperties = new JsonObject();
        var rootRequired = new JsonArray();

        foreach (var location in new[] { "path", "query" })
        {
            var selected = parameters.Where(parameter => parameter.Location == location).ToArray();
            if (selected.Length == 0)
            {
                continue;
            }

            var properties = new JsonObject();
            var required = new JsonArray();
            foreach (var parameter in selected)
            {
                properties[parameter.Name] = parameter.Schema.DeepClone();
                if (parameter.Required)
                {
                    required.Add(parameter.Name);
                }
            }

            var section = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["additionalProperties"] = false
            };
            if (required.Count > 0)
            {
                section["required"] = required;
                rootRequired.Add(location);
            }

            rootProperties[location] = section;
        }

        var result = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = rootProperties,
            ["additionalProperties"] = false
        };
        if (rootRequired.Count > 0)
        {
            result["required"] = rootRequired;
        }

        return result;
    }

    private static JsonObject BuildOutputSchema(
        JsonNode openApi,
        JsonObject operation,
        PilotContract contract)
    {
        var responseSchema = operation["responses"]?["200"]?["content"]?["application/json"]?["schema"]
            ?? throw new ContractCompilationException(
                $"Tool '{contract.ToolName}' has no JSON 200 response schema.");
        var envelope = ResolveSchema(openApi, responseSchema, contract.ToolName);
        var dataSchema = envelope["properties"]?["data"]
            ?? throw new ContractCompilationException(
                $"Tool '{contract.ToolName}' response has no data property.");
        dataSchema = ResolveSchema(openApi, dataSchema, contract.ToolName);
        if (dataSchema["type"]?.GetValue<string>() == "array")
        {
            dataSchema = ResolveSchema(openApi, dataSchema["items"]!, contract.ToolName);
        }

        var documentedFields = dataSchema["properties"] as JsonObject
            ?? throw new ContractCompilationException(
                $"Tool '{contract.ToolName}' response data is not an object schema.");
        var projectedFields = new JsonObject();
        foreach (var field in contract.AllowedDataFields)
        {
            if (!documentedFields.TryGetPropertyValue(field, out var fieldSchema) || fieldSchema is null)
            {
                throw new ContractCompilationException(
                    $"Allowed output field '{field}' is not documented for '{contract.ToolName}'.");
            }

            projectedFields[field] = fieldSchema.DeepClone();
        }

        return new JsonObject
        {
            ["type"] = "object",
            ["properties"] = projectedFields,
            ["additionalProperties"] = false
        };
    }

    private static JsonObject ResolveSchema(JsonNode openApi, JsonNode schema, string toolName)
    {
        var current = schema;
        var depth = 0;
        while (current is JsonObject objectSchema
            && objectSchema["$ref"]?.GetValue<string>() is { } reference)
        {
            if (++depth > 32 || !reference.StartsWith("#/", StringComparison.Ordinal))
            {
                throw new ContractCompilationException(
                    $"Tool '{toolName}' contains an unsupported schema reference '{reference}'.");
            }

            current = reference[2..]
                .Split('/')
                .Aggregate(openApi, (node, segment) => node[segment.Replace("~1", "/").Replace("~0", "~")]!)
                ?? throw new ContractCompilationException(
                    $"Tool '{toolName}' contains an unresolved schema reference '{reference}'.");
        }

        return current as JsonObject
            ?? throw new ContractCompilationException($"Tool '{toolName}' schema is not an object.");
    }

    private static JsonObject FindOpenApiOperation(
        JsonNode openApi,
        string path,
        string method,
        string toolName)
    {
        return openApi["paths"]?[path]?[method.ToLowerInvariant()] as JsonObject
            ?? throw new ContractCompilationException(
                $"OpenAPI operation {method} {path} for '{toolName}' was not found.");
    }

    private static void ValidateManifest(
        DiscoveryManifest manifest,
        byte[] openApiBytes,
        IReadOnlyCollection<OperationInventory> operations,
        IReadOnlyCollection<PilotContract> pilots)
    {
        if (manifest.FormatVersion != 1
            || manifest.Purpose != "discovery-only"
            || manifest.Executable)
        {
            throw new ContractCompilationException(
                "C3 accepts only formatVersion 1 discovery-only, non-executable manifests.");
        }

        var actualHash = Convert.ToHexString(SHA256.HashData(openApiBytes)).ToLowerInvariant();
        if (!string.Equals(actualHash, manifest.SchemaSha256, StringComparison.Ordinal))
        {
            throw new ContractCompilationException(
                $"OpenAPI SHA-256 mismatch. Expected {manifest.SchemaSha256}; found {actualHash}.");
        }

        if (manifest.OperationCount != operations.Count)
        {
            throw new ContractCompilationException(
                $"Manifest operation count {manifest.OperationCount} does not match inventory count {operations.Count}.");
        }

        if (manifest.PilotCandidateCount != pilots.Count)
        {
            throw new ContractCompilationException(
                $"Manifest pilot count {manifest.PilotCandidateCount} does not match contract count {pilots.Count}.");
        }
    }

    private static void ValidateEmbeddedPilotContract(JsonObject operation, PilotContract contract)
    {
        var embedded = operation["x-ix-pilot-contract"] as JsonObject
            ?? throw new ContractCompilationException(
                $"OpenAPI operation for '{contract.ToolName}' has no embedded pilot contract.");

        RequireString(embedded, "toolName", contract.ToolName, contract.ToolName);
        RequireString(embedded, "method", contract.Method, contract.ToolName);
        RequireString(embedded, "path", contract.Path, contract.ToolName);
        RequireString(embedded, "companyScope", contract.CompanyScope, contract.ToolName);

        if (embedded["metadataVersion"]?.GetValue<int>() != contract.MetadataVersion
            || embedded["contractVersion"]?.GetValue<int>() != contract.ContractVersion
            || embedded["executionEnabled"]?.GetValue<bool>() != contract.ExecutionEnabled
            || !ReadStringArray(embedded, "allowedQueryParameters").SequenceEqual(
                contract.AllowedQueryParameters,
                StringComparer.Ordinal)
            || !ReadStringArray(embedded, "allowedDataFields").SequenceEqual(
                contract.AllowedDataFields,
                StringComparer.Ordinal))
        {
            throw new ContractCompilationException(
                $"OpenAPI pilot metadata for '{contract.ToolName}' does not match pilot-contracts.json.");
        }
    }

    private static IReadOnlyList<string> ReadStringArray(JsonObject node, string property)
    {
        return (node[property] as JsonArray)?.Select(value => value?.GetValue<string>()
                ?? throw new ContractCompilationException(
                    $"'{property}' contains a non-string value."))
            .ToArray()
            ?? throw new ContractCompilationException($"'{property}' is missing or is not an array.");
    }

    private static void RequireString(
        JsonObject operation,
        string property,
        string expected,
        string toolName)
    {
        var actual = operation[property]?.GetValue<string>();
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new ContractCompilationException(
                $"OpenAPI property '{property}' for '{toolName}' must be '{expected}', found '{actual}'.");
        }
    }

    private static string RequiredText(JsonObject node, string property, string toolName) =>
        node[property]?.GetValue<string>()
        ?? throw new ContractCompilationException(
            $"'{property}' is missing from a parameter for '{toolName}'.");

    private static IEnumerable<string> PathPlaceholders(string path)
    {
        var start = 0;
        while ((start = path.IndexOf('{', start)) >= 0)
        {
            var end = path.IndexOf('}', start + 1);
            if (end < 0)
            {
                throw new ContractCompilationException($"Path '{path}' has an unmatched placeholder.");
            }

            yield return path[(start + 1)..end];
            start = end + 1;
        }
    }

    private static string RequiredFile(string directory, string fileName)
    {
        var path = Path.Combine(directory, fileName);
        return File.Exists(path)
            ? path
            : throw new ContractCompilationException($"Required contract file '{path}' was not found.");
    }

    private static async Task<T> ReadAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken)
                ?? throw new ContractCompilationException($"Contract file '{path}' is empty.");
        }
        catch (JsonException exception)
        {
            throw new ContractCompilationException($"Contract file '{path}' is invalid JSON: {exception.Message}");
        }
    }

    private sealed record ParameterDefinition(
        string Name,
        string Location,
        bool Required,
        JsonObject Schema);
}
