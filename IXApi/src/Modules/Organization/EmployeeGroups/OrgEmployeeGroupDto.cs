namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupDto
    {
        public int UserGroupID { get; set; }
        public string? Code { get; set; }
        public string UserGroupName { get; set; } = null!;
        public int? Module { get; set; }
    }
}