using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.LocalCache
{
    internal class LocalCacheToCacheInternalAdapter<TK, TV> : ICacheInternal<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;

        public LocalCacheToCacheInternalAdapter(ILocalCache<TK, TV> cache)
        {
            _cache = cache;

            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public ValueTask<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            return new ValueTask<GetFromCacheResult<TK, TV>>(_cache.Get(key));
        }

        public ValueTask Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            _cache.Set(key, value, timeToLive);
            
            return new ValueTask();
        }

        public ValueTask<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            return new ValueTask<IList<GetFromCacheResult<TK, TV>>>(_cache.Get(keys));
        }

        public ValueTask Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            _cache.Set(values, timeToLive);
            
            return new ValueTask();
        }
    }
}