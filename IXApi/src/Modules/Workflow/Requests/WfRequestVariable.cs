using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Workflow.Variables;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestVariable:Entity<long>
    {
        public long RequestId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestId))]
        public virtual WfRequest Request { get; set; } = null!;

        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual WfVariable Variable { get; set; } = null!;

        public string? VariableValue { get; set; }
    }
}


