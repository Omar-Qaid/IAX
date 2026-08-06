namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class AppPermissionDto
    {
        public int RecId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Key => $"{Module}.{Resource}.{Action}";
    }

    public class RolePermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<AppPermissionDto> Permissions { get; set; } = new();
    }

    public class AssignPermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> PermissionIds { get; set; } = new();
    }
}

