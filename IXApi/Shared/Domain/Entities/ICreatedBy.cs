using IAX.IXApi.Modules.Identity.Authentication;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Impersonation;

namespace IAX.IXApi.Shared.Domain.Entities
{
    public interface ICreatedBy
    {
        string CreatedBy { get; set; }
        public AspNetUser CreatedByUser { get; set; }
    }
}

