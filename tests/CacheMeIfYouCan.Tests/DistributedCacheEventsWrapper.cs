﻿using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests
{
    public class DistributedCacheEventsWrapper<TKey, TValue> : DistributedCacheEventsWrapperBase<TKey, TValue>
    {
        private readonly DistributedCacheEventsWrapperConfig<TKey, TValue> _config;

        public DistributedCacheEventsWrapper(
            DistributedCacheEventsWrapperConfig<TKey, TValue> config,
            IDistributedCache<TKey, TValue> innerCache)
            : base(innerCache)
        {
            _config = config;
        }

        protected override void OnTryGetCompletedSuccessfully(
            TKey key,
            bool resultSuccess,
            ValueAndTimeToLive<TValue> resultValue,
            TimeSpan duration)
        {
            _config.OnTryGetCompletedSuccessfully?.Invoke(key, resultSuccess, resultValue, duration);
        }

        protected override void OnTryGetException(
            TKey key,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnTryGetException?.Invoke(key, duration, exception) ?? false;
        }

        protected override void OnSetCompletedSuccessfully(
            TKey key,
            TValue value,
            TimeSpan timeToLive,
            TimeSpan duration)
        {
            _config.OnSetCompletedSuccessfully?.Invoke(key, value, timeToLive, duration);
        }

        protected override void OnSetException(
            TKey key,
            TValue value,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetException?.Invoke(key, value, timeToLive, duration, exception) ?? false;
        }

        protected override void OnGetManyCompletedSuccessfully(
            IReadOnlyCollection<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        {
            _config.OnGetManyCompletedSuccessfully?.Invoke(keys, values.ToArray(), duration);
        }

        protected override void OnGetManyException(
            IReadOnlyCollection<TKey> keys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnGetManyException?.Invoke(keys, duration, exception) ?? false;
        }

        protected override void OnSetManyCompletedSuccessfully(
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        {
            _config.OnSetManyCompletedSuccessfully?.Invoke(values, timeToLive, duration);
        }

        protected override void OnSetManyException(
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetManyException?.Invoke(values, timeToLive, duration, exception) ?? false;
        }
    }
    
    public class DistributedCacheEventsWrapper<TOuterKey, TInnerKey, TValue> : DistributedCacheEventsWrapperBase<TOuterKey, TInnerKey, TValue>
    {
        private readonly DistributedCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> _config;

        public DistributedCacheEventsWrapper(
            DistributedCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> config,
            IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache)
            : base(innerCache)
        {
            _config = config;
        }

        protected override void OnGetManyCompletedSuccessfully(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        {
            _config.OnGetManyCompletedSuccessfully?.Invoke(outerKey, innerKeys, values.ToArray(), duration);
        }

        protected override void OnGetManyException(
            TOuterKey outerKey,
            IReadOnlyCollection<TInnerKey> innerKeys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnGetManyException?.Invoke(outerKey, innerKeys, duration, exception) ?? false;
        }

        protected override void OnSetManyCompletedSuccessfully(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        {
            _config.OnSetManyCompletedSuccessfully?.Invoke(outerKey, values, timeToLive, duration);
        }

        protected override void OnSetManyException(
            TOuterKey outerKey,
            IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetManyException?.Invoke(outerKey, values, timeToLive, duration, exception) ?? false;
        }
    }
}