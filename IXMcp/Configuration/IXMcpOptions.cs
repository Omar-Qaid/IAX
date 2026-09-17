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
}
