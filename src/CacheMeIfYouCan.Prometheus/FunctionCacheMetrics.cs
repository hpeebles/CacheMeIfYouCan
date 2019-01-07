using System;

namespace CacheMeIfYouCan.Prometheus
{
    [Flags]
    public enum FunctionCacheMetrics
    {
        None = 0,
        GetResult = 1 << 0,
        Fetch = 1 << 1,
        All = Int32.MaxValue
    }
}