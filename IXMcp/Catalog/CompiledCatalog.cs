using System.Text.Json.Nodes;

namespace IAX.IXMcp.Catalog;

public sealed record CompiledCatalog(
    string SchemaSha256,
    string ApiAssemblyVersion,
    IReadOnlyList<CompiledTool> Tools)
{
    public static CompiledCatalog Empty { get; } = new(string.Empty, string.Empty, []);
}

public sealed record CompiledTool(
    string Name,
    int ContractVersion,
    string Module,
    string OperationId,
    string Method,
    string Path,
    bool ExecutionEnabled,
    string CompanyScope,
    JsonObject InputSchema,
    JsonObject OutputDataSchema,
    IReadOnlyList<BindingParameter> BindingParameters,
    IReadOnlyList<string> AllowedDataFields,
    IReadOnlyList<string> RequiredPermissions,
    string RecordAuthorization,
    IReadOnlyList<string> RequiredEvidence);

public sealed record BindingParameter(
    string Name,
    string Location,
    bool Required);
