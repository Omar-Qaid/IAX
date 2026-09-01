using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
public class WfActivityTypeDto : WfMasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}

