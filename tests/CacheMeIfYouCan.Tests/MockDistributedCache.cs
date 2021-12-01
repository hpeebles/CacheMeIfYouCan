using System;
using System.Collections.Generic;
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
        public int TryRemoveExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;

        public Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            ThrowIfRequested();
            
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
            
            ThrowIfRequested();
            
            _innerCache.Set(key, (value, DateTime.UtcNow + timeToLive), timeToLive);
            
            return Task.CompletedTask;
        }

        public Task<int> GetMany(ReadOnlyMemory<TKey> keys, Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);
            
            ThrowIfRequested();
            
            var resultsArray = new KeyValuePair<TKey, (TValue, DateTime)>[keys.Length];
            
            var countFound = _innerCache.GetMany(keys.Span, resultsArray);

            for (var i = 0; i < countFound; i++)
            {
                var (key, (value, expiry)) = resultsArray[i];
                var timeToLive = expiry - DateTime.UtcNow;
                destination.Span[i] = new KeyValuePair<TKey, ValueAndTimeToLive<TValue>>(key, new ValueAndTimeToLive<TValue>(value, timeToLive));
            }
            
            var hits = countFound;
            var misses = keys.Length - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);
                
            return Task.FromResult(hits);
        }

        public Task SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            ThrowIfRequested();
            
            var valuesInner = new KeyValuePair<TKey, (TValue, DateTime)>[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var (key, value) = values.Span[i];
                valuesInner[i] = new KeyValuePair<TKey, (TValue, DateTime)>(key, (value, DateTime.UtcNow + timeToLive));
            }
            
            _innerCache.SetMany(valuesInner, timeToLive);
            
            return Task.CompletedTask;
        }

        public Task<bool> TryRemove(TKey key)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            ThrowIfRequested();

            return Task.FromResult(_innerCache.TryRemove(key, out _));
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;

        private void ThrowIfRequested()
        {
            if (!_throwExceptionOnNextAction)
                return;
            
            _throwExceptionOnNextAction = false;
            throw new Exception();
        }
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
            ReadOnlyMemory<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            ThrowIfRequested();

            var resultsArray = new KeyValuePair<TInnerKey, (TValue, DateTime)>[innerKeys.Length];
            
            var countFound = _innerCache.GetMany(outerKey, innerKeys.Span, resultsArray);

            for (var i = 0; i < countFound; i++)
            {
                var (key, (value, expiry)) = resultsArray[i];
                var timeToLive = expiry - DateTime.UtcNow;
                destination.Span[i] = new KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>(
                    key,
                    new ValueAndTimeToLive<TValue>(value, timeToLive));
            }
            
            var hits = countFound;
            var misses = innerKeys.Length - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return Task.FromResult(hits);
        }

        public Task SetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetManyExecutionCount);

            ThrowIfRequested();

            var valuesInner = new KeyValuePair<TInnerKey, (TValue, DateTime)>[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var (key, value) = values.Span[i];
                valuesInner[i] = new KeyValuePair<TInnerKey, (TValue, DateTime)>(key, (value, DateTime.UtcNow + timeToLive));
            }
            
            _innerCache.SetMany(outerKey, valuesInner, timeToLive);

            return Task.CompletedTask;
        }

        public Task<bool> TryRemove(TOuterKey outerKey, TInnerKey innerKey)
        {
            Interlocked.Increment(ref SetManyExecutionCount);
            
            ThrowIfRequested();

            return Task.FromResult(_innerCache.TryRemove(outerKey, innerKey, out _));
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
        
        private void ThrowIfRequested()
        {
            if (!_throwExceptionOnNextAction)
                return;
            
            _throwExceptionOnNextAction = false;
            throw new Exception();
        }
    }
}