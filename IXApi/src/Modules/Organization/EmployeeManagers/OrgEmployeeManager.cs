using IAX.IXApi.Modules.Organization.Employees;
using IAX.IXApi.Modules.Organization.ManagementLevels;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Organization.EmployeeManagers
{
    /// <summary>
    /// Dynamic management hierarchy link: one row per (employee, management level) pointing at the
    /// employee who is the manager at that level. Replaces the old fixed Manager1..Manager4 columns.
    /// Composite key (EmployeeId, ManagementLevelId) => at most one manager per level per employee.
    /// </summary>
    public class OrgEmployeeManager
    {
        public long EmployeeId { get; set; }
        public byte ManagementLevelId { get; set; }
        public long ManagerId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker Employee { get; set; } = null!;

        [ForeignKey(nameof(ManagerId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker Manager { get; set; } = null!;

        [ForeignKey(nameof(ManagementLevelId))]
        public virtual OrgManagementLevel ManagementLevel { get; set; } = null!;
    }
}

