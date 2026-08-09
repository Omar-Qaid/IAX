using System.Collections.Generic;

namespace IAX.IXApi.Modules.Identity.Users
{
    /// <summary>Body for setting the exact set of roles a user should have (by role name).</summary>
    public class AssignRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }
}
