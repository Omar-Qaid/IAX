using IAX.IXApi.Modules.Organization.Employees;
using IAX.IXApi.Modules.Organization.Employees.Abstraction;
using IAX.IXApi.Modules.Organization.Departments;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Showrooms
{
    /// <summary>
    /// An independent organizational unit (point of sale). A showroom has many sellers (employees);
    /// each seller belongs to at most one showroom (Employee.ShowroomId). Shares the OrgEntities TPH
    /// table with OrgEmployee, so it can also back an Identity user via OrgEntity.
    /// </summary>
    public class OrgShowroom : OrgEntity
    {
        public string? Location { get; set; }
        public virtual OrgDepartment Department { get; set; } = null!;

        public virtual ICollection<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker> Sellers { get; set; } = new List<IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker>();
    }
}

