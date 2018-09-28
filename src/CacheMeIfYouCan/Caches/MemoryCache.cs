using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    public class MemoryCache<TK, TV> : ILocalCache<TK, TV>
    {
        private const string Type = "memory";
        private readonly MemoryCache _cache;
        
        internal MemoryCache(int maxSizeMB = 100)
        {
            var config = new NameValueCollection
            {
                { "cacheMemoryLimitMegabytes", maxSizeMB.ToString() }
            };
            
            _cache = new MemoryCache(Guid.NewGuid().ToString(), config);
        }

        public GetFromCacheResult<TV> Get(Key<TK> key)
        {
            var valueObj = _cache.Get(key.AsString.Value);

            GetFromCacheResult<TV> result;
            if (valueObj is ValueWithExpiry<TV> value)
                result = new GetFromCacheResult<TV>(value, value.Expiry - DateTimeOffset.UtcNow, Type);
            else
                result = GetFromCacheResult<TV>.NotFound;

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