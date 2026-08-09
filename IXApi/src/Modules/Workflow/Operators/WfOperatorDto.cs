using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Operators
{
    public class WfOperatorDto : MasterEntityDto<byte>
    {
        public byte SortOrder { get; set; }
    }
}
