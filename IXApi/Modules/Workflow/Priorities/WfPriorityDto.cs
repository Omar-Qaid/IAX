using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Priorities
{
    public class WfPriorityDto : MasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}
