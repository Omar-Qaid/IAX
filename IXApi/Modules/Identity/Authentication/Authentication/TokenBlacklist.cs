using IAX.IXApi.Shared.Application.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace IAX.IXApi.Modules.Identity.Authentication.Authentication
{
    [ScopedService]
    public class TokenBlacklist : ITokenBlacklist
    {
        private readonly IDistributedCache _cache;

        public TokenBlacklist(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task BlacklistAsync(string jti, TimeSpan ttl)
        {
            await _cache.SetStringAsync(GetCacheKey(jti), "revoked", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }

        public async Task<bool> IsBlacklistedAsync(string jti)
        {
            var value = await _cache.GetStringAsync(GetCacheKey(jti));
            return value != null;
        }

        private string GetCacheKey(string jti) => $"blacklist:{jti}";
    }
}
