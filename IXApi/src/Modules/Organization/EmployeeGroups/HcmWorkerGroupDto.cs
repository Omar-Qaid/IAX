namespace IAX.IXApi.Modules.Organization.Features.HcmWorkerGroup
{
    public class HcmWorkerGroupDto
    {
        public int UserGroupID { get; set; }
        public string? Code { get; set; }
        public string UserGroupName { get; set; } = null!;
        public int? Module { get; set; }
    }
}