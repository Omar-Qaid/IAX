using Microsoft.AspNetCore.Authorization;

namespace IAX.IXApi.Modules.Identity.Permissions
{
    /// <summary>
    /// Enforces a database-driven permission check on a controller or action.
    /// Usage: [RequirePermission("Inventory", " ", "View")]
    /// The full permission key is "Inventory. .View".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string module, string resource, string action)
            : base($"permission:{module}.{resource}.{action}") { }
    }
}
