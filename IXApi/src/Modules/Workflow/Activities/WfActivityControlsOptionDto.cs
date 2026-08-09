using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Activities
{
    public class WfActivityControlsOptionDto : BaseEntityDto<long>
    {
        public long ActivityControlId { get; set; }
        public string Value { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public string NameAr { get; set; } = null!;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
