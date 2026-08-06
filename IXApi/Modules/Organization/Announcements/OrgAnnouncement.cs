using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System;

namespace IAX.IXApi.Modules.Organization.Announcements
{
    public class OrgAnnouncement: MasterEntity<int>
    {
        public DateTime ExpiryDate { get; set; }
        public string? PhotoURL { get; set; }
    }
}
