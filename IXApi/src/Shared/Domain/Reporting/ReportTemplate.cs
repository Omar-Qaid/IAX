using System.ComponentModel.DataAnnotations;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Shared.Domain.Reporting;

public enum ReportTemplateStatus : byte
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public sealed class ReportTemplate : MasterEntity<long>
{
    [MaxLength(255)]
    public string? NameAlias { get; set; }
    public int RefTableId { get; set; }
    public long RefRecId { get; set; }
    public string PageSize { get; set; } = "A4";
    public string Orientation { get; set; } = "portrait";
    public string Language { get; set; } = "en";
    public bool IsDefault { get; set; }
    public ReportTemplateStatus Status { get; set; } = ReportTemplateStatus.Draft;
    public long? CurrentVersionId { get; set; }
    public ReportTemplateVersion? CurrentVersion { get; set; }
    public ICollection<ReportTemplateVersion> Versions { get; set; } = [];
}
