using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    public static class CachedObjectConfigurationManagerExtensions
    {
        public static IDistributedCache<TKey, TValue> WithApplicationInsightsTelemetry<TKey, TValue>(this IDistributedCache<TKey, TValue> distributedCache, 
            ITelemetryProcessor telemetryProcessor, ITelemetryConfig telemetryConfig, string hostName, string cacheName)
        {
            var redisTelemetry = new RedisCacheTelemetry(telemetryProcessor, telemetryConfig, hostName, cacheName);
            distributedCache.SetTelemetry(redisTelemetry);
            return distributedCache;
        }

        public static IDistributedCache<TOuterKey, TInnerKey, TValue> WithApplicationInsightsTelemetry<TOuterKey, TInnerKey, TValue>(this IDistributedCache<TOuterKey, TInnerKey, TValue> distributedCache,
            ITelemetryProcessor telemetryProcessor, ITelemetryConfig telemetryConfig, string hostName, string cacheName)
        {
            var redisTelemetry = new RedisCacheTelemetry(telemetryProcessor, telemetryConfig, hostName, cacheName);
            distributedCache.SetTelemetry(redisTelemetry);
            return distributedCache;
        }
    }
}
