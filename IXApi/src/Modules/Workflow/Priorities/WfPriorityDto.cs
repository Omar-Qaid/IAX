using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
public class WfPriorityDto : WfMasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}
