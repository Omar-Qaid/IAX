namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupMemberDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
    }
}