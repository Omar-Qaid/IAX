using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsOptionDto : EntityDto<long>
    {
        public long ActivityControlId { get; set; }
        public string Value { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
