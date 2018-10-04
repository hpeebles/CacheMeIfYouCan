using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.Internal
{
    internal class TwoTierCache<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _localCache;
        private readonly ICache<TK, TV> _remoteCache;

        public TwoTierCache(ILocalCache<TK, TV> localCache, ICache<TK, TV> remoteCache)
        {
            _localCache = localCache;
            _remoteCache = remoteCache;
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var results = new List<GetFromCacheResult<TK, TV>>(keys.Count);
            var remaining = new List<Key<TK>>();

            foreach (var key in keys)
            {
                var fromMemoryCache = _localCache.Get(key);
                
                if (fromMemoryCache.Success)
                    results.Add(fromMemoryCache);
                else
                    remaining.Add(key);
            }

            if (!remaining.Any())
                return results;
            
            var fromRemoteCache = await _remoteCache.Get(remaining);
            
            foreach (var result in fromRemoteCache)
                _localCache.Set(result.Key, result.Value, result.TimeToLive);

            results.AddRange(fromRemoteCache);
            
            return results;
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            foreach (var kv in values)
                _localCache.Set(kv.Key, kv.Value, timeToLive);

            await _remoteCache.Set(values, timeToLive);
        }
    }
}