using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace CacheMeIfYouCan.Internal
{
    internal class MemoryCache<T> : ICache<T>
    {
        private const string Type = "Memory";
        private readonly MemoryCache _cache;
        
        public MemoryCache(long maxItems)
        {
            _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = maxItems });
        }

        public Task<GetFromCacheResult<T>> Get(string key)
        {
            var result = _cache.TryGetValue(key, out ValueWithExpiry<T> value)
                ? new GetFromCacheResult<T>(value, value.Expiry - DateTimeOffset.UtcNow, Type)
                : GetFromCacheResult<T>.NotFound;

            return Task.FromResult(result);
        }

        public Task Set(string key, T value, TimeSpan timeToLive)
        {
            var expiry = DateTime.UtcNow + timeToLive;

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiry,
                Size = 1
            };
            
            _cache.Set(key, new ValueWithExpiry<T>(value, expiry), options);
            
            return Task.CompletedTask;
        }
    }
}