namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class RolePermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<AppPermissionDto> Permissions { get; set; } = new();
    }
}