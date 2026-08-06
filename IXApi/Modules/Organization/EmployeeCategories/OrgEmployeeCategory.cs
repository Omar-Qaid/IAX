using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public class OrgEmployeeCategory : MasterEntity<long>
    {
        public bool? ForAll { get; set; }
        public bool? Manager1 { get; set; }
        public bool? Manager2 { get; set; }
        public bool? Manager3 { get; set; }
        public bool? Manager4 { get; set; }

        /// <summary>
        /// Linkage rows — each pairs the category with one of Department / Occupation / OrgEmployeeGroup.
        /// Inverse side of OrgEmployeeCategoryGroup.UserCategoriesID; no schema change (FK already exists).
        /// </summary>
        public virtual ICollection<OrgEmployeeCategoryGroup> Groups { get; set; } = new List<OrgEmployeeCategoryGroup>();
    }
}

