using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class WfPrintTemplateVersion : Entity<long>
{
    public long TemplateId { get; set; }
    public WfPrintTemplate Template { get; set; } = null!;
    public int VersionNo { get; set; }
    public string TemplateJson { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
}
