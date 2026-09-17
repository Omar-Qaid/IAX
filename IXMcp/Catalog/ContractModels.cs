using System.Text.Json.Serialization;

namespace IAX.IXMcp.Catalog;

internal sealed record DiscoveryManifest(
    int FormatVersion,
    string Purpose,
    bool Executable,
    string ApiAssemblyVersion,
    string SchemaSha256,
    int OperationCount,
    int PilotCandidateCount);

internal sealed record PilotContract(
    int MetadataVersion,
    string ToolName,
    int ContractVersion,
    string Method,
    string Path,
    bool ExecutionEnabled,
    string CompanyScope,
    IReadOnlyList<string> AllowedQueryParameters,
    int? DefaultPageSize,
    int? MaximumPageSize,
    IReadOnlyList<string> AllowedDataFields,
    string RecordAuthorization,
    IReadOnlyList<string> RequiredEvidence);

internal sealed record OperationInventory(
    string OperationId,
    string Module,
    string Method,
    string Path,
    string? ProposedToolName,
    string Status,
    bool AllowsAnonymous,
    IReadOnlyList<string> DomainPermissions);

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(DiscoveryManifest))]
[JsonSerializable(typeof(PilotContract[]))]
[JsonSerializable(typeof(OperationInventory[]))]
internal sealed partial class ContractJsonContext : JsonSerializerContext;
