using System;
using System.Collections.Generic;
using System.Linq;
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
        public int RemoveExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;
        
        public bool TryGet(TKey key, out TValue value)
        {
            Interlocked.Increment(ref TryGetExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }
            
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

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            _innerCache.Set(key, value, timeToLive);
        }

        public IReadOnlyCollection<KeyValuePair<TKey, TValue>> GetMany(IReadOnlyCollection<TKey> keys)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

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
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            _innerCache.SetMany(values, timeToLive);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            Interlocked.Increment(ref RemoveExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            return _innerCache.TryRemove(key, out value);
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
    
    public class MockLocalCache<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache = new MemoryCache<TOuterKey, TInnerKey, TValue>(k => k.ToString(), k => k.ToString());

        public int GetManyExecutionCount;
        public int SetMany1ExecutionCount;
        public int SetMany2ExecutionCount;
        public int RemoveExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;

        public IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            var values = _innerCache.GetMany(outerKey, innerKeys);
            
            var hits = values.Count;
            var misses = innerKeys.Count - values.Count;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return values;
        }

        public void SetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            Interlocked.Increment(ref SetMany1ExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            _innerCache.SetMany(outerKey, values, timeToLive);
        }

        public void SetManyWithVaryingTimesToLive(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            Interlocked.Increment(ref SetMany2ExecutionCount);
            
            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            _innerCache.SetManyWithVaryingTimesToLive(outerKey, values);
        }

        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            Interlocked.Increment(ref RemoveExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            return _innerCache.TryRemove(outerKey, innerKey, out value);
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
}