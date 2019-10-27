using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;

namespace CacheMeIfYouCan.Internal.DistributedCache
{
    internal class DistributedCacheDuplicateRequestCatchingWrapper<TK, TV> : IDistributedCache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly KeyComparer<TK> _keyComparer;
        private readonly DuplicateTaskCatcherSingle<Key<TK>, GetFromCacheResult<TK, TV>> _getHandlerSingle;
        private readonly DuplicateTaskCatcherMulti<Key<TK>, GetFromCacheResult<TK, TV>> _getHandlerMulti;
        private const int DuplicateStatusCode = 11;

        public DistributedCacheDuplicateRequestCatchingWrapper(
            IDistributedCache<TK, TV> cache,
            KeyComparer<TK> keyComparer)
        {
            _cache = cache;
            _keyComparer = keyComparer;
            _getHandlerSingle = new DuplicateTaskCatcherSingle<Key<TK>, GetFromCacheResult<TK, TV>>((k, t) => _cache.Get(k), keyComparer);
            _getHandlerMulti = new DuplicateTaskCatcherMulti<Key<TK>, GetFromCacheResult<TK, TV>>((k, t) => GetMultiImpl(k), keyComparer);

            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
        }

        public string CacheName { get; }
        public string CacheType { get; }

        public void Dispose() => _cache.Dispose();
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            var (result, duplicate) = await _getHandlerSingle.ExecuteAsync(key, CancellationToken.None);

            return result.Value.WithStatusCode(duplicate ? DuplicateStatusCode : 0);
        }

        public Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            return _cache.Set(key, value, timeToLive);
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(IReadOnlyCollection<Key<TK>> keys)
        {
            var results = await _getHandlerMulti.ExecuteAsync(keys, CancellationToken.None);

            return results
                .Select(kv => kv.Value.Value.WithStatusCode(kv.Value.Duplicate ? DuplicateStatusCode : 0))
                .ToList();
        }

        public Task Set(IReadOnlyCollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            return _cache.Set(values, timeToLive);
        }

        public Task<bool> Remove(Key<TK> key)
        {
            return _cache.Remove(key);
        }

        private async Task<IDictionary<Key<TK>, GetFromCacheResult<TK, TV>>> GetMultiImpl(IReadOnlyCollection<Key<TK>> keys)
        {
            var results = await _cache.Get(keys);

            return results.ToDictionary(x => x.Key, x => x, _keyComparer);
        }
    }
}