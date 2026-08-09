using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Variables
{
    public class WfDataTypeDto : MasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}
