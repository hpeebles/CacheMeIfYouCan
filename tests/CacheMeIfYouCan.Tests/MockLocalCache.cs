using System;
using System.Collections.Generic;
using System.Threading;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Tests
{
    public class MockLocalCache<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache = new MemoryCache<TKey, TValue>(k => k.ToString());

        public int TryGetExecutionCount;
        public int SetExecutionCount;
        public int GetManyExecutionCount;
        public int SetManyExecutionCount;
        public int HitsCount;
        public int MissesCount;
        
        public bool TryGet(TKey key, out TValue value)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            if (_innerCache.TryGet(key, out value))
            {
                Interlocked.Increment(ref HitsCount);
                return true;
            }

            Interlocked.Increment(ref MissesCount);
            return false;
        }

        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetExecutionCount);

            _innerCache.Set(key, value, timeToLive);
        }

        public IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            var values = _innerCache.GetMany(keys);
            
            var hits = values.Count;
            var misses = keys.Count - values.Count;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return values;
        }

        public void SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            _innerCache.SetMany(values, timeToLive);
        }
    }
}