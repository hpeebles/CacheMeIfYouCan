using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    public class MemoryCache<TK, TV> : ILocalCache<TK, TV>
    {
        private const string CacheType = "memory";
        private readonly MemoryCache _cache;
        
        internal MemoryCache(int maxSizeMB = 100)
        {
            var config = new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", maxSizeMB.ToString() }
            };
            
            _cache = new MemoryCache(Guid.NewGuid().ToString(), config);
        }

        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            var valueObj = _cache.Get(key.AsString.Value);

            GetFromCacheResult<TK, TV> result;
            if (valueObj is ValueWithExpiry<TV> value)
                result = new GetFromCacheResult<TK, TV>(key, value, value.Expiry - DateTimeOffset.UtcNow, CacheType);
            else
                result = GetFromCacheResult<TK, TV>.NotFound(key);

            return result;
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            var expiry = DateTime.UtcNow + timeToLive;
            
            _cache.Set(key.AsString.Value, new ValueWithExpiry<TV>(value, expiry), expiry);
        }

        public void Remove(Key<TK> key)
        {
            _cache.Remove(key.AsString.Value);
        }
    }
}