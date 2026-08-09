using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityTypeDto : MasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}

