using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Tests
{
    public class MockDistributedCache<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, (TValue, DateTime)> _innerCache = new MemoryCache<TKey, (TValue, DateTime)>(k => k.ToString());
        
        public int TryGetExecutionCount;
        public int SetExecutionCount;
        public int GetManyExecutionCount;
        public int SetManyExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;

        public Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
            if (_innerCache.TryGet(key, out var value))
            {
                Interlocked.Increment(ref HitsCount);
                return Task.FromResult((true, new ValueAndTimeToLive<TValue>(value.Item1, value.Item2 - DateTime.UtcNow)));
            }
            
            Interlocked.Increment(ref MissesCount);
            return Task.FromResult((false, new ValueAndTimeToLive<TValue>()));
        }

        public Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
            _innerCache.Set(key, (value, DateTime.UtcNow + timeToLive), timeToLive);
            
            return Task.CompletedTask;
        }

        public Task<int> GetMany(IReadOnlyCollection<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
            var resultsArray = new KeyValuePair<TKey, (TValue, DateTime)>[keys.Count];
            
            var countFound = _innerCache.GetMany(keys, resultsArray);

            for (var i = 0; i < countFound; i++)
            {
                var (key, (value, expiry)) = resultsArray[i];
                var timeToLive = expiry - DateTime.UtcNow;
                destination.Span[i] = new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>(key, new ValueAndTimeToLive<TValue>(value, timeToLive));
            }
            
            var hits = countFound;
            var misses = keys.Count - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);
                
            return Task.FromResult(hits);
        }

        public Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
            _innerCache.SetMany(values.ToDictionary(kv => kv.Key, kv => (kv.Value, DateTime.UtcNow + timeToLive)), timeToLive);
            
            return Task.CompletedTask;
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
    
    public class MockDistributedCache<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, (TValue, DateTime)> _innerCache = new MemoryCache<TOuterKey, TInnerKey, (TValue, DateTime)>(k => k.ToString(), k => k.ToString());

        public int GetManyExecutionCount;
        public int SetManyExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;

        public Task<int> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            var resultsArray = new KeyValuePair<TInnerKey, (TValue, DateTime)>[innerKeys.Count];
            
            var countFound = _innerCache.GetMany(outerKey, innerKeys, resultsArray);

            for (var i = 0; i < countFound; i++)
            {
                var (key, (value, expiry)) = resultsArray[i];
                var timeToLive = expiry - DateTime.UtcNow;
                destination.Span[i] = new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>(
                    key,
                    new ValueAndTimeToLive<TValue>(value, timeToLive));
            }
            
            var hits = countFound;
            var misses = innerKeys.Count - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return Task.FromResult(hits);
        }

        public Task SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
            _innerCache.SetMany(outerKey, values.ToDictionary(kv => kv.Key, kv => (kv.Value, DateTime.UtcNow + timeToLive)), timeToLive);

            return Task.CompletedTask;
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
}