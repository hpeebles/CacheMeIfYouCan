using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
            var stopwatch = Stopwatch.StartNew();

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
            var stopwatch = Stopwatch.StartNew();

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
            IReadOnlyCollection<TKey> keys,
            Memory<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> destination)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var countFound = await _innerCache
                    .GetMany(keys, destination)
                    .ConfigureAwait(false);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(keys, values.Span, stopwatch.Elapsed);

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
            IReadOnlyCollection<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        { }

        protected virtual void OnGetManyException(
            IReadOnlyCollection<TKey> keys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion

        #region SetMany
        public async Task SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _innerCache
                    .SetMany(values, timeToLive)
                    .ConfigureAwait(false);

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
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        { }

        protected virtual void OnSetManyException(
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
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
            IReadOnlyCollection<TInnerKey> innerKeys,
            Memory<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> destination)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var countFound = await _innerCache
                    .GetMany(outerKey, innerKeys, destination)
                    .ConfigureAwait(false);

                var values = destination.Slice(0, countFound);

                OnGetManyCompletedSuccessfully(outerKey, innerKeys, values.Span, stopwatch.Elapsed);

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
            IReadOnlyCollection<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        { }

        protected virtual void OnGetManyException(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
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
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _innerCache
                    .SetMany(outerKey, values, timeToLive)
                    .ConfigureAwait(false);

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
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        { }

        protected virtual void OnSetManyException(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = false;
        }
        #endregion
    }
}