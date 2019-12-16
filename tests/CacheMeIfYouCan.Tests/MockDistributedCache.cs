﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Tests
{
    public class MockDistributedCache<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, (TValue, DateTime)> _innerCache = new MemoryCache<TKey, (TValue, DateTime)>(k => k.ToString());
        
        public int TryGetExecutionCount = 0;
        public int SetExecutionCount = 0;
        public int GetManyExecutionCount = 0;
        public int SetManyExecutionCount = 0;

        public Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            return _innerCache.TryGet(key, out var value)
                ? Task.FromResult((true, new ValueAndTimeToLive<TValue>(value.Item1, value.Item2 - DateTime.UtcNow)))
                : Task.FromResult((false, new ValueAndTimeToLive<TValue>()));
        }

        public Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetExecutionCount);
            
            _innerCache.Set(key, (value, DateTime.UtcNow + timeToLive), timeToLive);
            
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            Interlocked.Increment(ref GetManyExecutionCount);
            
            var values = _innerCache
                .GetMany(keys)
                .ToDictionary(kv => kv.Key, kv => new ValueAndTimeToLive<TValue>(kv.Value.Item1, kv.Value.Item2 - DateTime.UtcNow));

            return Task.FromResult<IReadOnlyCollection<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>>>(values);
        }

        public Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            _innerCache.SetMany(values.ToDictionary(kv => kv.Key, kv => (kv.Value, DateTime.UtcNow + timeToLive)), timeToLive);
            
            return Task.CompletedTask;
        }
    }
}