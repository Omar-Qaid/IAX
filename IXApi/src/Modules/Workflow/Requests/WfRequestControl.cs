using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Processes;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Workflow.Requests
{
public class WfRequestControl: WfMasterEntity<long>
    {
        public long ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public byte ControlId { get; set; }
        [ForeignKey(nameof(ControlId))]
        public virtual WfControl Control { get; set; } = null!;
        public decimal Score { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }  // as  xml 
        public string? ExtendedProperties { get; set; }  // as  xml 
    }
}

