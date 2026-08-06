using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Organization.Departments;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Organization.Employees.Abstraction
{
    public abstract class OrgEntity : MasterEntity<long>
    {
        public short DepartmentId { get; set; }
        public long PartyId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual OrgDepartment Department { get; set; } = null!;

        public virtual AspNetUser? User { get; set; }
    }
}

