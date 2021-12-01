using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    public sealed class DistributedCacheApplicationInsightsWrapper<TKey, TValue> 
        : DistributedCacheEventsWrapperBase<TKey, TValue>
    {
        private readonly TelemetryProcessor _telemetryProcessor;

        public DistributedCacheApplicationInsightsWrapper(IDistributedCache<TKey, TValue> innerCache,
            IDistributedCacheConfig cacheConfig, 
            ITelemetryProcessor telemetryProcessor, 
            ITelemetryConfig telemetryConfig) 
            : base(innerCache)
        {
            _telemetryProcessor = new TelemetryProcessor(cacheConfig, telemetryProcessor, telemetryConfig);
        }

        protected override void OnTryGetCompletedSuccessfully(TKey key, bool resultSuccess,
            ValueAndTimeToLive<TValue> resultValue, TimeSpan duration)
        {
            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", $"Key '{key}'", true);

            base.OnTryGetCompletedSuccessfully(key, resultSuccess, resultValue, duration);
        }

        protected override void OnSetManyCompletedSuccessfully(ReadOnlySpan<KeyValuePair<TKey, TValue>> values,
            TimeSpan timeToLive, TimeSpan duration)
        {
            var keys = $"Keys '{string.Join(",", values.ToArray().Select(d => d.Key))}'";

            _telemetryProcessor.Add(duration, "StringSetAsync", keys, true);

            base.OnSetManyCompletedSuccessfully(values, timeToLive, duration);
        }

        protected override void OnGetManyCompletedSuccessfully(ReadOnlySpan<TKey> keys,
            ReadOnlySpan<KeyValuePair<TKey, ValueAndTimeToLive<TValue>>> values, TimeSpan duration)
        {
            var keysText = $"Keys '{string.Join(",", keys.ToArray())}'";

            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", keysText, true);

            base.OnGetManyCompletedSuccessfully(keys, values, duration);
        }

        protected override void OnGetManyException(ReadOnlySpan<TKey> keys, TimeSpan duration, Exception exception,
            out bool exceptionHandled)
        {
            var keysText = $"Keys '{string.Join(",", keys.ToArray())}'";

            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", keysText, false);

            base.OnGetManyException(keys, duration, exception, out exceptionHandled);
        }

        protected override void OnSetCompletedSuccessfully(TKey key, TValue value, TimeSpan timeToLive,
            TimeSpan duration)
        {
            _telemetryProcessor.Add(duration, "StringSetAsync", $"Key '{key}'", true);

            base.OnSetCompletedSuccessfully(key, value, timeToLive, duration);
        }

        protected override void OnSetException(TKey key, TValue value, TimeSpan timeToLive, TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            _telemetryProcessor.Add(duration, "StringSetAsync", $"Key '{key}'", false);

            base.OnSetException(key, value, timeToLive, duration, exception, out exceptionHandled);
        }

        protected override void OnSetManyException(ReadOnlySpan<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive,
            TimeSpan duration, Exception exception,
            out bool exceptionHandled)
        {
            var keys = $"Keys '{string.Join(",", values.ToArray().Select(d => d.Key))}'";

            _telemetryProcessor.Add(duration, "StringSetAsync", keys, false);

            base.OnSetManyException(values, timeToLive, duration, exception, out exceptionHandled);
        }

        protected override void OnTryGetException(TKey key, TimeSpan duration, Exception exception,
            out bool exceptionHandled)
        {
            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", $"Key '{key}'", false);

            base.OnTryGetException(key, duration, exception, out exceptionHandled);
        }

        protected override void OnTryRemoveCompletedSuccessfully(TKey key, bool wasRemoved, TimeSpan duration)
        {
            _telemetryProcessor.Add(duration, "KeyDeleteAsync", $"Key '{key}'", true);

            base.OnTryRemoveCompletedSuccessfully(key, wasRemoved, duration);
        }

        protected override void OnTryRemoveException(TKey key, TimeSpan duration, Exception exception,
            out bool exceptionHandled)
        {
            _telemetryProcessor.Add(duration, "KeyDeleteAsync", $"Key '{key}'", false);

            base.OnTryRemoveException(key, duration, exception, out exceptionHandled);
        }
    }

    public sealed class DistributedCacheApplicationInsightsWrapper<TOuterKey, TInnerKey, TValue> 
        : DistributedCacheEventsWrapperBase<TOuterKey, TInnerKey, TValue>
    {
        private readonly TelemetryProcessor _telemetryProcessor;

        public DistributedCacheApplicationInsightsWrapper(IDistributedCache<TOuterKey, TInnerKey, TValue> innerCache,
            IDistributedCacheConfig cacheConfig, 
            ITelemetryProcessor telemetryProcessor,
            ITelemetryConfig telemetryConfig) : base(innerCache)
        {
            _telemetryProcessor = new TelemetryProcessor(cacheConfig, telemetryProcessor, telemetryConfig);
        }

        protected override void OnSetManyException(TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive, TimeSpan duration,
            Exception exception, out bool exceptionHandled)
        {
            var keys =
                $"Keys {string.Join(",", values.ToArray().Select(innerKeyValue => $"'{outerKey}.{innerKeyValue.Key}'"))}";

            _telemetryProcessor.Add(duration, "StringSetAsync", keys, false);

            base.OnSetManyException(outerKey, values, timeToLive, duration, exception, out exceptionHandled);
        }

        protected override void OnTryRemoveCompletedSuccessfully(TOuterKey outerKey, TInnerKey innerKey,
            bool wasRemoved, TimeSpan duration)
        {
            _telemetryProcessor.Add(duration, "KeyDeleteAsync", $"Key '{outerKey}.{innerKey}'", true);

            base.OnTryRemoveCompletedSuccessfully(outerKey, innerKey, wasRemoved, duration);
        }

        protected override void OnTryRemoveException(TOuterKey outerKey, TInnerKey innerKey, TimeSpan duration,
            Exception exception,
            out bool exceptionHandled)
        {
            _telemetryProcessor.Add(duration, "KeyDeleteAsync", $"Key '{outerKey}.{innerKey}'", false);

            base.OnTryRemoveException(outerKey, innerKey, duration, exception, out exceptionHandled);
        }

        protected override void OnGetManyException(TOuterKey outerKey, ReadOnlySpan<TInnerKey> innerKeys,
            TimeSpan duration, Exception exception,
            out bool exceptionHandled)
        {
            var keys = $"Keys {string.Join(",", innerKeys.ToArray().Select(innerKey => $"'{outerKey}.{innerKey}'"))}";

            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", keys, false);

            base.OnGetManyException(outerKey, innerKeys, duration, exception, out exceptionHandled);
        }

        protected override void OnSetManyCompletedSuccessfully(TOuterKey outerKey,
            ReadOnlySpan<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive, TimeSpan duration)
        {
            var keys =
                $"Keys {string.Join(",", values.ToArray().Select(innerKeyValue => $"'{outerKey}.{innerKeyValue.Key}'"))}";

            _telemetryProcessor.Add(duration, "StringSetAsync", keys, true);

            base.OnSetManyCompletedSuccessfully(outerKey, values, timeToLive, duration);
        }

        protected override void OnGetManyCompletedSuccessfully(TOuterKey outerKey, ReadOnlySpan<TInnerKey> innerKeys,
            ReadOnlySpan<KeyValuePair<TInnerKey, ValueAndTimeToLive<TValue>>> values, TimeSpan duration)
        {
            var keys = $"Keys {string.Join(",", innerKeys.ToArray().Select(innerKey => $"'{outerKey}.{innerKey}'"))}";

            _telemetryProcessor.Add(duration, "StringGetWithExpiryAsync", keys, true);

            base.OnGetManyCompletedSuccessfully(outerKey, innerKeys, values, duration);
        }
    }
}
