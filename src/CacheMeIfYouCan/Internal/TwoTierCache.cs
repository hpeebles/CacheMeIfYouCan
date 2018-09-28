using System;
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

        public async Task<GetFromCacheResult<TV>> Get(Key<TK> key)
        {
            var fromMemoryCache = _localCache.Get(key);
                
            if (fromMemoryCache.Success)
                return fromMemoryCache;

            var fromRemoteCache = await _remoteCache.Get(key);
            
            if (!fromRemoteCache.Success)
                return GetFromCacheResult<TV>.NotFound;

            _localCache.Set(key, fromRemoteCache.Value, fromRemoteCache.TimeToLive);

            return fromRemoteCache;
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            _localCache.Set(key, value, timeToLive);

            await _remoteCache.Set(key, value, timeToLive);
        }
    }
}