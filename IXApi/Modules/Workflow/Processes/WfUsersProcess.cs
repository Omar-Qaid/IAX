using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Organization.Departments;

using IAX.IXApi.Modules.Organization.Occupations;

namespace IAX.IXApi.Modules.Workflow.Processes
{
    public class WfUsersProcess : Entity<long>
    {
        public long ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;

        public short? DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public virtual OrgDepartment? Department { get; set; }

        public short? OccupationId { get; set; }
        [ForeignKey(nameof(OccupationId))]
        public virtual OrgOccupation? Occupation { get; set; }

        public long? EmployeeId { get; set; }
        [ForeignKey(nameof(EmployeeId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker? Employee { get; set; }
    }
}

