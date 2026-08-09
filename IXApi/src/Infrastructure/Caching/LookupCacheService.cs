using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace IAX.IXApi.Infrastructure.Caching
{
    public interface ILookupCacheService
    {
        Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null);
        void Remove(string cacheKey);
    }

    public class LookupCacheService : ILookupCacheService
    {
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

        public LookupCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            if (value != null)
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration,
                    SlidingExpiration = TimeSpan.FromMinutes(3)
                };
                _cache.Set(cacheKey, value, options);
            }

            return value;
        }

        public void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
