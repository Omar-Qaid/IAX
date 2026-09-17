using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.ManagementLevels
{
    /// <summary>
    /// [Unused / Legacy] A tier in the management hierarchy (e.g. Supervisor, Area Manager, Region Manager, General Manager).
    /// Kept for backward compatibility; organization hierarchy structure is managed via OrganizationHierarchyNode.
    /// </summary>
    public class HcmWorkerManagementLevel : MasterEntity<byte>
    {
        public byte Level { get; set; }
    }
}
