using System;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Identity.Authentication.Authentication
{
    public interface ITokenBlacklist
    {
        Task BlacklistAsync(string jti, TimeSpan ttl);
        Task<bool> IsBlacklistedAsync(string jti);
    }
}
