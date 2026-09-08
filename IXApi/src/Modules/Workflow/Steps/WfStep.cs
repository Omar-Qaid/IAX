using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Workflow.Processes;

namespace IAX.IXApi.Modules.Workflow.Steps
{
    [DataManagement]
    public class WfStep : WfMasterEntity<long>
    {
        public long ProcessId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public byte SortOrder { get; set; }
        public decimal Score { get; set; }
        public bool MustCompleteAll { get; set; }
        public bool IsSystemDefined  { get; set; }
    }
}

