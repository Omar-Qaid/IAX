using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    /// <summary>
    /// A tier in the management hierarchy (e.g. Supervisor, Area Manager, Region Manager, General
    /// Manager). Levels are data-driven so the hierarchy depth is dynamic.
    /// </summary>
    public class OrgManagementLevel : MasterEntity<byte>
    {
        public byte Level { get; set; }
    }
}


