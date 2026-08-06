using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityMappingVariableDto : BaseEntityDto<long>
    {
        public long ActivityControlId { get; set; }
        public long VariableId { get; set; }
        public byte VariableOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
