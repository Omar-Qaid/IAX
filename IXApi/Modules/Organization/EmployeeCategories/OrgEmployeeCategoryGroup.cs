using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Modules.Organization.Occupations;
using UserGroupEntity = IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup.OrgEmployeeGroup;

namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeCategory
{
    public class OrgEmployeeCategoryGroup : MasterEntity<long>
    {
        public long UserCategoriesID { get; set; }
        [ForeignKey(nameof(UserCategoriesID))]
        public virtual OrgEmployeeCategory OrgEmployeeCategory { get; set; } = null!;

        public short? DepartmentID { get; set; }
        [ForeignKey(nameof(DepartmentID))]
        public virtual OrgDepartment? Department { get; set; }

        public short? OccupationID { get; set; }
        [ForeignKey(nameof(OccupationID))]
        public virtual OrgOccupation? Occupation { get; set; }

        public long? UserGroupID { get; set; }
        [ForeignKey(nameof(UserGroupID))]
        public virtual UserGroupEntity? OrgEmployeeGroup { get; set; }
    }
}



