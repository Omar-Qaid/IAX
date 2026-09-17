using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Modules.Organization.Departments
{
    /// <summary>
    /// [Unused / Potentially Redundant] Legacy Department entity.
    /// Kept for backward compatibility; prefer OrganizationUnit with Type = Department.
    /// </summary>
    public class Department : MasterEntity<short>
    {
    }
}

