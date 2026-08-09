namespace IAX.IXApi.Modules.Identity.Permissions
{
    public class AssignPermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public List<int> PermissionIds { get; set; } = new();
    }
}