using IAX.IXApi.Modules.Organization.Employees;
using IAX.IXApi.Modules.Organization.ManagementLevels;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Organization.HcmWorkerManagers
{
    /// <summary>
    /// [Unused / Redundant] Legacy dynamic worker-to-manager link table.
    /// Kept for backward compatibility; worker-manager reporting structures are managed via OrganizationHierarchyNode.
    /// </summary>
    public class HcmWorkerManager
    {
        public long EmployeeId { get; set; }
        public byte ManagementLevelId { get; set; }
        public long ManagerId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker Employee { get; set; } = null!;

        [ForeignKey(nameof(ManagerId))]
        public virtual IAX.IXApi.Modules.Organization.Employees.Entities.HcmWorker Manager { get; set; } = null!;

        [ForeignKey(nameof(ManagementLevelId))]
        public virtual HcmWorkerManagementLevel ManagementLevel { get; set; } = null!;
    }
}

