using System;

namespace CacheMeIfYouCan.Prometheus
{
    [Flags]
    public enum CacheMetrics
    {
        None = 0b_0001,
        Get = 0b_0010,
        Set = 0b_0100,
        Remove = 0b_1000,
        Exception = 0b_0001_0000,
        All = Int32.MaxValue
    }
}