using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class MemoryCache<T> : ICache<T>
    {
        private const string Type = "Memory";
        private readonly MemoryCache _cache;
        
        internal MemoryCache(int maxSizeMB)
        {
            var config = new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", maxSizeMB.ToString() }
            };
            
            _cache = new MemoryCache(Guid.NewGuid().ToString(), config);
        }

        public Task<GetFromCacheResult<T>> Get(string key)
        {
            var valueObj = _cache.Get(key);

            GetFromCacheResult<T> result;
            if (valueObj is ValueWithExpiry<T> value)
                result = new GetFromCacheResult<T>(value, value.Expiry - DateTimeOffset.UtcNow, Type);
            else
                result = GetFromCacheResult<T>.NotFound;

            return Task.FromResult(result);
        }

        public Task Set(string key, T value, TimeSpan timeToLive)
        {
            var expiry = DateTime.UtcNow + timeToLive;
            
            _cache.Set(key, new ValueWithExpiry<T>(value, expiry), expiry);
            
            return Task.FromResult<object>(null);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}