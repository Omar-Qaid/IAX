using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public enum WfPrintTemplateStatus : byte
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public sealed class WfPrintTemplate : WfMasterEntity<long>
{
    public long ProcessId { get; set; }
    public WfProcess Process { get; set; } = null!;
    public string PageSize { get; set; } = "A4";
    public string Orientation { get; set; } = "portrait";
    public string Language { get; set; } = "en";
    public bool IsDefault { get; set; }
    public WfPrintTemplateStatus Status { get; set; } = WfPrintTemplateStatus.Draft;
    public long? CurrentVersionId { get; set; }
    public WfPrintTemplateVersion? CurrentVersion { get; set; }
    public ICollection<WfPrintTemplateVersion> Versions { get; set; } = [];
}
