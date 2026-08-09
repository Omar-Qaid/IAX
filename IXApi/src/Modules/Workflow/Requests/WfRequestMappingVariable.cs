using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestMappingVariable : Entity<long>
    {
        public long RequestControlId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RequestControlId))]
        public virtual WfRequestControl RequestControl { get; set; } = null!;

        public long VariableId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(VariableId))]
        public virtual IAX.IXApi.Modules.Workflow.Variables.WfVariable Variable { get; set; } = null!;

        public byte SortOrder { get; set; }
    }
}



