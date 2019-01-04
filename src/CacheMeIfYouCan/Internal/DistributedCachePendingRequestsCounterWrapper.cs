using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class DistributedCachePendingRequestsCounterWrapper<TK, TV> : IDistributedCache<TK, TV>, IPendingRequestsCounter
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private int _pendingRequestsCount;

        public DistributedCachePendingRequestsCounterWrapper(IDistributedCache<TK, TV> cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            CacheName = cache.CacheName;
            CacheType = cache.CacheType;
            
            PendingRequestsCounterContainer.Add(this);
        }

        public string CacheName { get; }
        public string CacheType { get; }

        string IPendingRequestsCounter.Name => CacheName;
        string IPendingRequestsCounter.Type => CacheType;

        public int PendingRequestsCount => _pendingRequestsCount;

        public void Dispose()
        {
            PendingRequestsCounterContainer.Remove(this);
            _cache.Dispose();
        }
        
        public async Task<GetFromCacheResult<TK, TV>> Get(Key<TK> key)
        {
            return await Execute(() => _cache.Get(key));
        }

        public async Task Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            await Execute(() => _cache.Set(key, value, timeToLive));
        }

        public async Task<IList<GetFromCacheResult<TK, TV>>> Get(ICollection<Key<TK>> keys)
        {
            return await Execute(() => _cache.Get(keys));
        }

        public async Task Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            await Execute(() => _cache.Set(values, timeToLive));
        }

        private async Task Execute(Func<Task> action)
        {
            Interlocked.Increment(ref _pendingRequestsCount);

            try
            {
                await action();
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequestsCount);
            }
        }
        
        private async Task<T> Execute<T>(Func<Task<T>> func)
        {
            Interlocked.Increment(ref _pendingRequestsCount);

            try
            {
                return await func();
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequestsCount);
            }
        }
    }
}