using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Requests
{
    public class WfRequestControlsOptionDto : EntityDto<long>
    {
        public long RequestControlId { get; set; }
        public string Value { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Score { get; set; }
        public int SortOrder { get; set; }
        public string? ExtendedProperties { get; set; }
    }
}
