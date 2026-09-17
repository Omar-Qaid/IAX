using System.ComponentModel.DataAnnotations;

namespace IAX.IXMcp.Configuration;

public sealed class IXMcpOptions
{
    public const string SectionName = "IXMcp";

    [Required]
    public string ContractDirectory { get; init; } = string.Empty;

    [Required]
    [Url]
    public string IXApiBaseUrl { get; init; } = string.Empty;

    [Required]
    public string ApprovedPathPrefix { get; init; } = "/api/";

    [Range(1, 300)]
    public int CallTimeoutSeconds { get; init; } = 30;

    [Range(1024, 4 * 1024 * 1024)]
    public int MaximumInputBytes { get; init; } = 256 * 1024;

    [Range(2048, 8 * 1024 * 1024)]
    public int MaximumRequestBodyBytes { get; init; } = 512 * 1024;

    [Range(1024, 16 * 1024 * 1024)]
    public int MaximumResponseBytes { get; init; } = 1024 * 1024;

    [Range(8, 128)]
    public int MaximumJsonDepth { get; init; } = 64;

    public string[] EnabledReadTools { get; init; } = [];

    public string[] EmergencyDeniedTools { get; init; } = [];

    public string[] AllowedOrigins { get; init; } = [];

    [Range(5, 3600)]
    public int CatalogRefreshSeconds { get; init; } = 60;

    [Range(1, 32)]
    public int MaximumConcurrentCallsPerUser { get; init; } = 4;
}
