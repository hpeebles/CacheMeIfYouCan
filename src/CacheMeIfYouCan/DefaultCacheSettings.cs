using System;

namespace CacheMeIfYouCan
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.MaxValue;
        public static int MemoryCacheMaxSizeMB = Int32.MaxValue;
        public static int MaxConcurrentFetches = Int32.MaxValue;
        public static bool EarlyFetchEnabled = true;
        public static Action<FunctionCacheErrorEvent> OnError = null;
    }
}