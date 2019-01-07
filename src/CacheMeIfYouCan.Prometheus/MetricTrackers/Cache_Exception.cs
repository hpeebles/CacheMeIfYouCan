using CacheMeIfYouCan.Notifications;
using Prometheus;

namespace CacheMeIfYouCan.Prometheus.MetricTrackers
{
    internal static class Cache_Exception
    {
        private static readonly Counter ExceptionsCounter;
        private const string NullString = "null";
        
        static Cache_Exception()
        {
            ExceptionsCounter = Metrics.CreateCounter("Cache_ExceptionsCounter", null, "name", "cachetype", "exceptiontype", "innerexceptiontype");
        }

        public static void OnCacheException(CacheException exception)
        {
            var labels = new[]
            {
                exception.CacheName,
                exception.CacheType,
                exception.GetType().Name,
                exception.InnerException?.GetType().Name ?? NullString
            };
            
            ExceptionsCounter
                .Labels(labels)
                .Inc();
        }
    }
}