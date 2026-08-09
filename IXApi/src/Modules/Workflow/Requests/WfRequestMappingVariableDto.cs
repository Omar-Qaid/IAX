using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestMappingVariableDto : BaseEntityDto<long>
    {
        public long RequestControlId { get; set; }
        public long VariableId { get; set; }
        public byte SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
