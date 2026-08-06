using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformer: LookupEntity<long>
    {
        public short PerformerTypeId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(PerformerTypeId))]
        public virtual WfPerformerType PerformerType { get; set; } = null!;
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
    }
}
