using System;

namespace CacheMeIfYouCan.Prometheus
{
    [Flags]
    public enum CacheMetrics
    {
        None = 0,
        Get = 1 << 0,
        Set = 1 << 1,
        Exception = 1 << 1,
        All = Int32.MaxValue
    }
}