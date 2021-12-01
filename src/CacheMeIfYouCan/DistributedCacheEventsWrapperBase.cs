using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public abstract class DistributedCacheEventsWrapperBase<TKey, TValue> : IDistributedCache<TKey, TValue>
    {
        private readonly IDistributedCache<TKey, TValue> _innerCache;

        protected DistributedCacheEventsWrapperBase(IDistributedCache<TKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        #region TryGet
        public async Task<(bool Success, ValueAndTimeToLive<TValue> Value)> TryGet(TKey key)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var result = await _innerCache
                    .TryGet(key)
                    .ConfigureAwait(false);

                OnTryGetCompletedSuccessfully(key, result.Success, result.Value, stopwatch.Elapsed);

                return result;
            }
            catch (Exception ex)
            {
                OnTryGetException(key, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return default;
            }
        }

        protected virtual void OnTryGetCompletedSuccessfully(
            TKey key,
            bool resultSuccess,
            ValueAndTimeToLive<TValue> resultValue,
            TimeSpan duration)
        { }
        #endregion

        protected virtual void OnTryGetException(
            TKey key,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        
        #region Set
        public async Task Set(TKey key, TValue value, TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                await _innerCache
                    .Set(key, value, timeToLive)
                    .ConfigureAwait(false);

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
        public async Task<int> GetMany(
            ReadOnlyMemory<TKey> keys,
            Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var countFound = await _innerCache
                    .GetMany(keys, destination)
                    .ConfigureAwait(false);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(keys.Span, values.Span, stopwatch.Elapsed);

                return countFound;
            }
            catch (Exception ex)
            {
                OnGetManyException(keys.Span, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return 0;
            }
        }

        protected virtual void OnGetManyCompletedSuccessfully(
            ReadOnlySpan<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> values,
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
        public async Task SetMany(ReadOnlyMemory<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                await _innerCache
                    .SetMany(values, timeToLive)
                    .ConfigureAwait(false);

                OnSetManyCompletedSuccessfully(values.Span, timeToLive, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetManyException(values.Span, timeToLive, stopwatch.Elapsed, ex, out var exceptionHandled);
                
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
        public async Task<bool> TryRemove(TKey key)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var wasRemoved = await _innerCache
                    .TryRemove(key)
                    .ConfigureAwait(false);

                OnTryRemoveCompletedSuccessfully(key, wasRemoved, stopwatch.Elapsed);

                return wasRemoved;
            }
            catch (Exception ex)
            {
                OnTryRemoveException(key, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return false;
            }
        }

        protected virtual void OnTryRemoveCompletedSuccessfully(
            TKey key,
            bool wasRemoved,
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
    
    public abstract class DistributedCacheEventsWrapperBase<TOuterKey, TInnerKey, TValue> : IDistributedCache<TOuterKey, TInnerKey, TValue>
    {
        private readonly IDistributedCache<TOuterKey, TInnerKey, TValue> _innerCache;

        protected DistributedCacheEventsWrapperBase(IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache)
        {
            _innerCache = innerCache;
        }

        #region GetMany
        public async Task<int> GetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var countFound = await _innerCache
                    .GetMany(outerKey, innerKeys, destination)
                    .ConfigureAwait(false);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(outerKey, innerKeys.Span, values.Span, stopwatch.Elapsed);

                return countFound;
            }
            catch (Exception ex)
            {
                OnGetManyException(outerKey, innerKeys.Span, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return 0;
            }
        }

        protected virtual void OnGetManyCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
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
        public async Task SetMany(
            TOuterKey outerKey,
            ReadOnlyMemory<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                await _innerCache
                    .SetMany(outerKey, values, timeToLive)
                    .ConfigureAwait(false);

                OnSetManyCompletedSuccessfully(outerKey, values.Span, timeToLive, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                OnSetManyException(outerKey, values.Span, timeToLive, stopwatch.Elapsed, ex, out var exceptionHandled);
                
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
        
        #region TryRemove
        public async Task<bool> TryRemove(TOuterKey outerKey, TInnerKey innerKey)
        {
            var stopwatch = StopwatchStruct.StartNew();

            try
            {
                var wasRemoved = await _innerCache
                    .TryRemove(outerKey, innerKey)
                    .ConfigureAwait(false);

                OnTryRemoveCompletedSuccessfully(outerKey, innerKey, wasRemoved, stopwatch.Elapsed);

                return wasRemoved;
            }
            catch (Exception ex)
            {
                OnTryRemoveException(outerKey, innerKey, stopwatch.Elapsed, ex, out var exceptionHandled);
                
                if (!exceptionHandled)
                    throw;

                return false;
            }
        }

        protected virtual void OnTryRemoveCompletedSuccessfully(
            TOuterKey outerKey,
            TInnerKey innerKey,
            bool wasRemoved,
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