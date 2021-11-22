using Microsoft.ApplicationInsights.Extensibility;

namespace CacheMeIfYouCan.Redis
{
    public static class CachedObjectConfigurationManagerExtensions
    {
        public static IDistributedCache<TKey, TValue> WithAppInsightsTelemetry<TKey, TValue>(this IDistributedCache<TKey, TValue> redisCache, 
            ITelemetryProcessor telemetryProcessor, IRedisTelemetryConfig redisTelemetryConfig, string hostName, string cacheName)
        {
            var redisTelemetry = new RedisTelemetry(telemetryProcessor, redisTelemetryConfig, hostName, cacheName);
            redisCache.SetTelemetry(redisTelemetry);
            return redisCache;
        }

        public static IDistributedCache<TOuterKey, TInnerKey, TValue> WithAppInsightsTelemetry<TOuterKey, TInnerKey, TValue>(this IDistributedCache<TOuterKey, TInnerKey, TValue> redisCache,
            ITelemetryProcessor telemetryProcessor, IRedisTelemetryConfig redisTelemetryConfig, string hostName, string cacheName)
        {
            var redisTelemetry = new RedisTelemetry(telemetryProcessor, redisTelemetryConfig, hostName, cacheName);
            redisCache.SetTelemetry(redisTelemetry);
            return redisCache;
        }
    }
}
