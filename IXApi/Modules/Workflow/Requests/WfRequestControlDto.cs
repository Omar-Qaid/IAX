using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Processes;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlDto : MasterEntityDto<long>
    {
        public long ProcessId { get; set; }
        public byte ControlId { get; set; }

        public bool Mandatory { get; set; }
        public bool UniqueKey { get; set; }
        public decimal Score { get; set; }
        public bool UsedAsCriteria { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }
        public string? ExtendedProperties { get; set; }

        public WfProcessDto? Process { get; set; } = null;
        public WfControlDto? Control { get; set; } = null;
    }
}
