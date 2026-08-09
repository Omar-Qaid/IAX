using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Variables;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfProcessVariable: Entity<long>
    {
        public long RequestId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestId))]
        public virtual WfRequest Request { get; set; } = null!;

        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual WfVariable Variable { get; set; } = null!;

        public string? VariableValue { get; set; }
        public byte SortOrder { get; set; }
    }
}



