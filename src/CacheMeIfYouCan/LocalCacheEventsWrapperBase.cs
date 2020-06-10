using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public abstract class LocalCacheEventsWrapperBase<TKey, TValue> : ILocalCache<TKey, TValue>
    {
        private readonly ILocalCache<TKey, TValue> _innerCache;

        protected LocalCacheEventsWrapperBase(ILocalCache<TKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public int Count => _innerCache.Count;
        public void Clear() => _innerCache.Clear();
        
        #region TryGet
        public bool TryGet(TKey key, out TValue value)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var found = _innerCache.TryGet(key, out value);

                OnTryGetCompletedSuccessfully(key, found, value, stopwatch.Elapsed);

                return found;
            }
            catch (Exception ex)
            {
                OnTryGetException(key, stopwatch.Elapsed, ex, out var exceptionHandled);

                if (!exceptionHandled)
                    throw;

                value = default;
                return false;
            }
        }

        protected virtual void OnTryGetCompletedSuccessfully(
            TKey key,
            bool found,
            TValue value,
            TimeSpan duration)
        { }

        protected virtual void OnTryGetException(
            TKey key,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region Set
        public void Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                _innerCache.Set(key, value, timeToLive);

                OnSetCompletedSuccessfully(key, value, timeToLive, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetException(key, value, timeToLive, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;
            }
        }

        protected virtual void OnSetCompletedSuccessfully(
            TKey key,
            TValue value,
            TimeSpan timeToLive,
            TimeSpan duration)
        { }

        protected virtual void OnSetException(
            TKey key,
            TValue value,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion
        
        #region GetMany
        public int GetMany(ReadOnlySpan<TKey> keys, Span<KeyValuePair<TKey, TValue>> destination)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var countFound = _innerCache.GetMany(keys, destination);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(keys, values, stopwatch.Elapsed);

                return countFound;
            }
            catch (Exception ex)
            {
                OnGetManyException(keys, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return 0;
            }
        }

        protected virtual void OnGetManyCompletedSuccessfully(
            ReadOnlySpan<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan duration)
        { }

        protected virtual void OnGetManyException(
            ReadOnlySpan<TKey> keys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region SetMany
        public void SetMany(ReadOnlySpan<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                _innerCache.SetMany(values, timeToLive);

                OnSetManyCompletedSuccessfully(values, timeToLive, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetManyException(values, timeToLive, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;
            }
        }

        protected virtual void OnSetManyCompletedSuccessfully(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        { }

        protected virtual void OnSetManyException(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region TryRemove
        public bool TryRemove(TKey key, out TValue value)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var removed = _innerCache.TryRemove(key, out value);

                OnTryRemoveCompletedSuccessfully(key, removed, value, stopwatch.Elapsed);

                return removed;
            }
            catch (Exception ex)
            {
                OnTryRemoveException(key, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                value = default;
                return false;
            }
        }

        protected virtual void OnTryRemoveCompletedSuccessfully(
            TKey key,
            bool removed,
            TValue value,
            TimeSpan duration)
        { }

        protected virtual void OnTryRemoveException(
            TKey key,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion
    }
    
    public abstract class LocalCacheEventsWrapperBase<TOuterKey, TInnerKey, TValue> : ILocalCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly ILocalCache<TOuterKey, TInnerKey, TValue> _innerCache;

        protected LocalCacheEventsWrapperBase(ILocalCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        public int Count => _innerCache.Count;
        public void Clear() => _innerCache.Clear();

        #region GetMany
        public int GetMany(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            Span<KeyValuePair<TInnerKey, TValue>> destination)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var countFound = _innerCache.GetMany(outerKey, innerKeys, destination);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(outerKey, innerKeys, values, stopwatch.Elapsed);

                return countFound;
            }
            catch (Exception ex)
            {
                OnGetManyException(outerKey, innerKeys, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return 0;
            }
        }

        protected virtual void OnGetManyCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan duration)
        { }

        protected virtual void OnGetManyException(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion
        
        #region SetMany
        public void SetMany(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                _innerCache.SetMany(outerKey, values, timeToLive);

                OnSetManyCompletedSuccessfully(outerKey, values, timeToLive, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetManyException(outerKey, values, timeToLive, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;
            }
        }

        protected virtual void OnSetManyCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        { }

        protected virtual void OnSetManyException(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region SetManyWithVaryingTimesToLive
        public void SetManyWithVaryingTimesToLive(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                _innerCache.SetManyWithVaryingTimesToLive(outerKey, values);

                OnSetManyWithVaryingTimesToLiveCompletedSuccessfully(outerKey, values, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetManyWithVaryingTimesToLiveException(
                    outerKey,
                    values,
                    stopwatch.Elapsed,
                    ex,
                    out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;
            }
        }

        protected virtual void OnSetManyWithVaryingTimesToLiveCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        { }

        protected virtual void OnSetManyWithVaryingTimesToLiveException(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region TryRemove
        public bool TryRemove(TOuterKey outerKey, TInnerKey innerKey, out TValue value)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var removed = _innerCache.TryRemove(outerKey, innerKey, out value);

                OnTryRemoveCompletedSuccessfully(outerKey, innerKey, removed, value, stopwatch.Elapsed);

                return removed;
            }
            catch (Exception ex)
            {
                OnTryRemoveException(outerKey, innerKey, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                value = default;
                return false;
            }
        }

        protected virtual void OnTryRemoveCompletedSuccessfully(
            TOuterKey outerKey,
            TInnerKey innerKey,
            bool removed,
            TValue value,
            TimeSpan duration)
        { }

        protected virtual void OnTryRemoveException(
            TOuterKey outerKey,
            TInnerKey innerKey,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion
    }
}