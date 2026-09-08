using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Steps
{
public class WfStepDto : WfMasterEntityDto<long>
    {
        public long ProcessId { get; set; }
        public byte SortOrder { get; set; }
        public decimal Score { get; set; }
        public bool MustCompleteAll { get; set; }
        public bool IsSystemDefined  { get; set; }
    }
}
