using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class TwoTierCache<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _localCache;
        private readonly IDistributedCache<TK, TV> _distributedCache;
        private readonly IEqualityComparer<Key<TK>> _keyComparer;

        public TwoTierCache(
            ILocalCache<TK, TV> localCache,
            IDistributedCache<TK, TV> distributedCache,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            _localCache = localCache;
            _distributedCache = distributedCache;
            _keyComparer = keyComparer;

            if (_distributedCache is IKeyChangeNotifier<TK> notifier)
                notifier.KeyChanges.Subscribe(_localCache.Remove);

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
            _localCache.Set(key, value, timeToLive);

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
                _localCache.Set(result.Key, result.Value, result.TimeToLive);

            return fromLocalCache
                .Concat(fromDistributedCache)
                .ToArray();
        }

        public async ValueTask Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            _localCache.Set(values, timeToLive);

            await _distributedCache.Set(values, timeToLive);
        }
    }
}