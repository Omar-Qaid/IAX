using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlDto : MasterEntityDto<long>
    {
        public long ActivityId { get; set; }
        public long ProcessId { get; set; }
        public byte ControlId { get; set; }
        public bool Mandatory { get; set; }
        public bool UniqueKey { get; set; }
        public decimal Score { get; set; }
        public bool UsedAsCriteria { get; set; }
        public bool UsedInSearch { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }
        public string? ExtendedProperties { get; set; }
    }
}
