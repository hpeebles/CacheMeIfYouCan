using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    public static class CachedObjectConfigurationManagerExtensions
    {
        public static IDistributedCache<TKey, TValue> WithApplicationInsightsTelemetry<TKey, TValue>(
            this IDistributedCache<TKey, TValue> distributedCache,
            IDistributedCacheConfig distributedCacheConfig,
            ITelemetryProcessor telemetryProcessor,
            ITelemetryConfig telemetryConfig)
        {
            return new DistributedCacheApplicationInsightsWrapper<TKey, TValue>(distributedCache,
                distributedCacheConfig, telemetryProcessor, telemetryConfig);
        }

        public static IDistributedCache<TOuterKey, TInnerKey, TValue> WithApplicationInsightsTelemetry<TOuterKey, TInnerKey, TValue>(
            this IDistributedCache<TOuterKey, TInnerKey, TValue> distributedCache,
            IDistributedCacheConfig distributedCacheConfig,
            ITelemetryProcessor telemetryProcessor,
            ITelemetryConfig telemetryConfig)
        {
            return new DistributedCacheApplicationInsightsWrapper<TOuterKey, TInnerKey, TValue>(distributedCache,
                distributedCacheConfig, telemetryProcessor, telemetryConfig);
        }
    }
}
