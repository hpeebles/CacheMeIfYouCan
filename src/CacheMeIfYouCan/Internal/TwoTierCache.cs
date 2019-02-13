using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class TwoTierCache<TK, TV> : ICacheInternal<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _localCache;
        private readonly IDistributedCache<TK, TV> _distributedCache;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly Func<TimeSpan, TimeSpan> _getLocalCacheTimeToLive;

        public TwoTierCache(
            ILocalCache<TK, TV> localCache,
            IDistributedCache<TK, TV> distributedCache,
            KeyComparer<TK> keyComparer,
            TimeSpan? localCacheTimeToLiveOverride)
        {
            _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _keyComparer = keyComparer ?? throw new ArgumentNullException(nameof(keyComparer));

            if (localCacheTimeToLiveOverride.HasValue)
            {
                var timeToLiveOverride = localCacheTimeToLiveOverride.Value;
                _getLocalCacheTimeToLive = t => t < timeToLiveOverride ? t : timeToLiveOverride;
            }
            else
            {
                _getLocalCacheTimeToLive = t => t;
            }

            CacheName = _localCache.CacheName;
            CacheType = $"{_localCache.CacheType}+{_distributedCache.CacheType}";
        }

        public string CacheName { get; }
        public string CacheType { get; }
        
        public async ValueTask<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var fromLocalCache = _localCache.Get(key);
            
            if (fromLocalCache.Success)
                return fromLocalCache;

            var fromDistributedCache = await _distributedCache.Get(key);
            
            if (fromDistributedCache.Success)
                _localCache.Set(key, fromDistributedCache.Value, fromDistributedCache.TimeToLive);

            return fromDistributedCache;
        }

        public async ValueTask Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            _localCache.Set(key, value, _getLocalCacheTimeToLive(timeToLive));

            await _distributedCache.Set(key, value, timeToLive);
        }

        public async ValueTask<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            var fromLocalCache = _localCache.Get(keys) ?? new GetFromCacheResult<TK, TV>[0];
            
            if (fromLocalCache.Count == keys.Count)
                return fromLocalCache;

            var remaining = keys
                .Except(fromLocalCache.Select(c => c.Key), _keyComparer)
                .ToArray();

            if (!remaining.Any())
                return fromLocalCache;
            
            var fromDistributedCache = await _distributedCache.Get(remaining);
            
            foreach (var result in fromDistributedCache)
                _localCache.Set(result.Key, result.Value, _getLocalCacheTimeToLive(result.TimeToLive));

            return fromLocalCache
                .Concat(fromDistributedCache)
                .ToArray();
        }

        public async ValueTask Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            _localCache.Set(values, _getLocalCacheTimeToLive(timeToLive));

            await _distributedCache.Set(values, timeToLive);
        }

        public async ValueTask Remove(Key<TK> key)
        {
            _localCache.Remove(key);

            await _distributedCache.Remove(key);
        }
    }
}