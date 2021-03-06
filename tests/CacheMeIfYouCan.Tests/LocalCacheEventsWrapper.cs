﻿using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Tests
{
    public class LocalCacheEventsWrapper<TKey, TValue> : LocalCacheEventsWrapperBase<TKey, TValue>
    {
        private readonly LocalCacheEventsWrapperConfig<TKey, TValue> _config;

        public LocalCacheEventsWrapper(
        LocalCacheEventsWrapperConfig<TKey, TValue> config,
            ILocalCache<TKey, TValue> innerCache)
            : base(innerCache)
        {
            _config = config;
        }

        protected override void OnTryGetCompletedSuccessfully(TKey key, bool found, TValue value, TimeSpan duration)
        {
            _config.OnTryGetCompletedSuccessfully?.Invoke(key, found, value, duration);
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
            ReadOnlySpan<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan duration)
        {
            _config.OnGetManyCompletedSuccessfully?.Invoke(keys.ToArray(), values.ToArray(), duration);
        }

        protected override void OnGetManyException(
            ReadOnlySpan<TKey> keys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnGetManyException?.Invoke(keys.ToArray(), duration, exception) ?? false;
        }

        protected override void OnSetManyCompletedSuccessfully(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        {
            _config.OnSetManyCompletedSuccessfully?.Invoke(values.ToArray(), timeToLive, duration);
        }

        protected override void OnSetManyException(
            ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetManyException?.Invoke(values.ToArray(), timeToLive, duration, exception) ?? false;
        }

        protected override void OnTryRemoveCompletedSuccessfully(
            TKey key,
            bool removed,
            TValue value,
            TimeSpan duration)
        {
            _config.OnTryRemoveCompletedSuccessfully?.Invoke(key, removed, value, duration);
        }

        protected override void OnTryRemoveException(
            TKey key,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnTryRemoveException?.Invoke(key, duration, exception) ?? false;
        }
    }
    
    public class LocalCacheEventsWrapper<TOuterKey, TInnerKey, TValue> : LocalCacheEventsWrapperBase<TOuterKey, TInnerKey, TValue>
    {
        private readonly LocalCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> _config;

        public LocalCacheEventsWrapper(
        LocalCacheEventsWrapperConfig<TOuterKey, TInnerKey, TValue> config,
            ILocalCache<TOuterKey, TInnerKey, TValue> innerCache)
            : base(innerCache)
        {
            _config = config;
        }

        protected override void OnGetManyCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan duration)
        {
            _config.OnGetManyCompletedSuccessfully?.Invoke(outerKey, innerKeys.ToArray(), values.ToArray(), duration);
        }

        protected override void OnGetManyException(
            TOuterKey outerKey,
            ReadOnlySpan<TInnerKey> innerKeys,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnGetManyException?.Invoke(outerKey, innerKeys.ToArray(), duration, exception) ?? false;
        }

        protected override void OnSetManyCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration)
        {
            _config.OnSetManyCompletedSuccessfully?.Invoke(outerKey, values.ToArray(), timeToLive, duration);
        }

        protected override void OnSetManyException(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values,
            TimeSpan timeToLive,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetManyException?.Invoke(outerKey, values.ToArray(), timeToLive, duration, exception) ?? false;
        }

        protected override void OnSetManyWithVaryingTimesToLiveCompletedSuccessfully(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration)
        {
            _config.OnSetManyWithVaryingTimesToLiveCompletedSuccessfully?.Invoke(outerKey, values.ToArray(), duration);
        }

        protected override void OnSetManyWithVaryingTimesToLiveException(
            TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnSetManyWithVaryingTimesToLiveException?.Invoke(outerKey, values.ToArray(), duration, exception) ?? false;
        }

        protected override void OnTryRemoveCompletedSuccessfully(
            TOuterKey outerKey,
            TInnerKey innerKey,
            bool removed,
            TValue value,
            TimeSpan duration)
        {
            _config.OnTryRemoveCompletedSuccessfully?.Invoke(outerKey, innerKey, removed, value, duration);
        }

        protected override void OnTryRemoveException(
            TOuterKey outerKey,
            TInnerKey innerKey,
            TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            exceptionHandled = _config.OnTryRemoveException?.Invoke(outerKey, innerKey, duration, exception) ?? false;
        }
    }
}