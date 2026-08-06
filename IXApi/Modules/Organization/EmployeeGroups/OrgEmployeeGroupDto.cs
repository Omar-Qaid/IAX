namespace IAX.IXApi.Modules.Organization.Features.OrgEmployeeGroup
{
    public class OrgEmployeeGroupDto
    {
        public int UserGroupID { get; set; }
        public string? Code { get; set; }
        public string UserGroupName { get; set; } = null!;
        public int? Module { get; set; }
    }

    /// <summary>A user that is (or could be) a member of a user group.</summary>
    public class OrgEmployeeGroupMemberDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
    }

    /// <summary>Body for adding/removing users to/from a group.</summary>
    public class AssignUsersDto
    {
        public List<string> UserIds { get; set; } = new();
    }
}

