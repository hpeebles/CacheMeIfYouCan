using System;
using System.Collections.Generic;
using System.Threading;

namespace CacheMeIfYouCan.Internal.LocalCache
{
    internal class LocalCachePendingRequestsCounterWrapper<TK, TV> : ILocalCache<TK, TV>, IPendingRequestsCounter
    {
        private readonly ILocalCache<TK, TV> _cache;
        private int _pendingRequestsCount;

        public LocalCachePendingRequestsCounterWrapper(ILocalCache<TK, TV> cache)
        {
            _cache = cache;

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
        
        public GetFromCacheResult<TK, TV> Get(Key<TK> key)
        {
            return Execute(() => _cache.Get(key));
        }

        public void Set(Key<TK> key, TV value, TimeSpan timeToLive)
        {
            Execute(() => _cache.Set(key, value, timeToLive));
        }

        public IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys)
        {
            return Execute(() => _cache.Get(keys));
        }

        public void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive)
        {
            Execute(() => _cache.Set(values, timeToLive));
        }

        public bool Remove(Key<TK> key)
        {
            return Execute(() => _cache.Remove(key));
        }

        private void Execute(Action action)
        {
            Interlocked.Increment(ref _pendingRequestsCount);

            try
            {
                action();
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequestsCount);
            }
        }
        
        private T Execute<T>(Func<T> func)
        {
            Interlocked.Increment(ref _pendingRequestsCount);

            try
            {
                return func();
            }
            finally
            {
                Interlocked.Decrement(ref _pendingRequestsCount);
            }
        }
    }
}