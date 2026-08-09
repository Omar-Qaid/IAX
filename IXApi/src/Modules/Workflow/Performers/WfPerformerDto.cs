using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerDto : MasterEntityDto<long>
    {
        public short PerformerTypeId { get; set; }
        public long? RelatedField { get; set; }
        public bool IsApplicant { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsManager1 { get; set; }
        public bool IsManager2 { get; set; }
        public bool IsManager3 { get; set; }
        public bool IsManager4 { get; set; }

        public string? SqlTable { get; set; }
        public string? SqlField { get; set; }
        public string? SqlWhere { get; set; }

        public List<long> UserIds { get; set; } = new();
    }
}
