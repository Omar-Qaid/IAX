using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Shared.Domain.Reporting;

public sealed class ReportEntityVersion : Entity<long>
{
    [System.ComponentModel.DataAnnotations.StringLength(255)]
    public string? Name { get; set; }
    [System.ComponentModel.DataAnnotations.StringLength(255)]
    public string? NameAlias { get; set; }
    public int RefTableId { get; set; }
    public long RefRecId { get; set; }
    public long TemplateId { get; set; }
    public ReportTemplate Template { get; set; } = null!;
    public long TemplateVersionId { get; set; }
    public ReportTemplateVersion TemplateVersion { get; set; } = null!;
    public DateTime SelectedAt { get; set; }
    public string SelectedBy { get; set; } = string.Empty;
}
