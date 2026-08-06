using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;

namespace IAX.IXApi.Modules.Workflow.Execution
{
    public class WfTransferDetails : Entity<long>
    {
        public long TransferId { get; set; }
        public long OldAssignmentId { get; set; }
        public long NewAssignmentId { get; set; }
        public long FromEmployeeId { get; set; }
        public long ToEmployeeId { get; set; }
        public DateTime AssignDate { get; set; }
    }
}

