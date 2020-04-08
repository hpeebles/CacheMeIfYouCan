using System;
using System.Collections.Generic;
using System.Threading;

namespace CacheMeIfYouCan.Tests
{
    public class MockLocalCache<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private ILocalCache<TKey, TValue> _innerCache = new MemoryCache<TKey, TValue>(k => k.ToString());

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

        public int GetMany(IReadOnlyCollection<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            var countFound = _innerCache.GetMany(keys, destination);
            
            var hits = countFound;
            var misses = keys.Count - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return countFound;
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
        
        public void Clear() => _innerCache = new MemoryCache<TKey, TValue>(k => k.ToString());

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
    
    public class MockLocalCache<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache = new MemoryCache<TOuterKey, TInnerKey, TValue>(k => k.ToString(), k => k.ToString());

        public int GetManyExecutionCount;
        public int SetMany1ExecutionCount;
        public int SetMany2ExecutionCount;
        public int RemoveExecutionCount;
        public int HitsCount;
        public int MissesCount;
        private bool _throwExceptionOnNextAction;

        public int GetMany(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            Interlocked.Increment(ref GetManyExecutionCount);

            if (_throwExceptionOnNextAction)
            {
                _throwExceptionOnNextAction = false;
                throw new Exception();
            }

            var countFound = _innerCache.GetMany(outerKey, innerKeys, destination);
            
            var hits = countFound;
            var misses = innerKeys.Count - countFound;

            if (hits > 0) Interlocked.Add(ref HitsCount, hits);
            if (misses > 0) Interlocked.Add(ref MissesCount, misses);

            return countFound;
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

        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
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

        public void Clear()
        {
            _innerCache = new MemoryCache<TOuterKey, TInnerKey, TValue>(k => k.ToString(), k => k.ToString());
        }

        public void ThrowExceptionOnNextAction() => _throwExceptionOnNextAction = true;
    }
}