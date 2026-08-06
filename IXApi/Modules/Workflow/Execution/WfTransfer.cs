using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    public class WfTransfer : Entity<long>
    {
        public long AssignmentId { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? Reason { get; set; }
    }
}
