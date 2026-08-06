using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Companies
{
    public class OrgCompany : MasterEntity<short>
    {
        public string? PrimaryEmail { get; set; }
        public string? NotificationEmail { get; set; }
        public string? IdentityUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorTheme { get; set; }
        public string? ColorMode { get; set; }
    }
}
