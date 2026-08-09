using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Organization.Companies
{
    public class OrgCompanyDto : MasterEntityDto<short>
    {
        public string? PrimaryEmail { get; set; }
        public string? NotificationEmail { get; set; }
        public string? IdentityUrl { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorTheme { get; set; }
        public string? ColorMode { get; set; }
    }
}
