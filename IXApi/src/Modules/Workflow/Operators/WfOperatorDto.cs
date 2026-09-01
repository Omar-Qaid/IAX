using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Operators
{
public class WfOperatorDto : WfMasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}
