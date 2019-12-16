using System;
using System.Collections.Generic;
using System.Threading;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Tests
{
    public class MockLocalCache<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache = new MemoryCache<TKey, TValue>(k => k.ToString());

        public int TryGetExecutionCount = 0;
        public int SetExecutionCount = 0;
        public int GetManyExecutionCount = 0;
        public int SetManyExecutionCount = 0;
        
        public bool TryGet(TKey key, out TValue value)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            return _innerCache.TryGet(key, out value);
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetExecutionCount);

            _innerCache.Set(key, value, timeToLive);
        }

        public IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            return _innerCache.GetMany(keys);
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            _innerCache.SetMany(values, timeToLive);
        }
    }
}