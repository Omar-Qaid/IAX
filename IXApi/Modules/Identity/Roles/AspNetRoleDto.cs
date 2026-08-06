using IAX.IXApi.Shared.Application.Contracts;

namespace IAX.IXApi.Modules.Identity.Roles
{
    public class AspNetRoleDto : EntityDto<string>
    {
        public string Name { get; set; } = string.Empty;
        public string? NormalizedName { get; set; }
        public string? ConcurrencyStamp { get; set; }
    }
}

