using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerTypeDto : EntityDto<short>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NameAlias { get; set; }
        public string? NameAR { get; set; }
        public byte SortOrder { get; set; }
    }
}
