using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Steps
{
    public class WfStepDto : MasterEntityDto<long>
    {
        public long ProcessId { get; set; }
        public byte SortOrder { get; set; }
        public decimal Score { get; set; }
        public byte AutoPassingHrs { get; set; }
        public bool AllMandatory { get; set; }
        public bool SysField { get; set; }
    }
}
