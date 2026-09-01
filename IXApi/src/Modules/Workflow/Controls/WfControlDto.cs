using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Controls
{
public class WfControlDto : WfMasterEntityDto<byte>
    {
        public string ControlType { get; set; } = null!;
        public byte SortOrder { get; set; }
    }
}
