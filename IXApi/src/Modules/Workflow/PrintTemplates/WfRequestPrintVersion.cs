using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.PrintTemplates;

public sealed class WfRequestPrintVersion : Entity<long>
{
    public long RequestId { get; set; }
    public WfRequest Request { get; set; } = null!;
    public long TemplateId { get; set; }
    public WfPrintTemplate Template { get; set; } = null!;
    public long TemplateVersionId { get; set; }
    public WfPrintTemplateVersion TemplateVersion { get; set; } = null!;
    public DateTime SelectedAt { get; set; }
    public string SelectedBy { get; set; } = string.Empty;
}
