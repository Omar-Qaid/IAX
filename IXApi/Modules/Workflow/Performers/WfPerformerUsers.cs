using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Workflow.Performers
{
    public class WfPerformerUsers : Entity<long>
    {
        public long PerformerId { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(PerformerId))]
        public virtual WfPerformer Performer { get; set; } = null!;

        public long UserID { get; set; }
        public long RelatedField { get; set; }
        public string? ExtendedProperties { get; set; }
    }
}



