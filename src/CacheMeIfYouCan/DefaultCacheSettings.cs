using System;

namespace CacheMeIfYouCan
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.FromDays(1);
        public static int MemoryCacheMaxSizeMB = 1024;
        public static int MaxConcurrentFetches = 1000;
        public static bool EarlyFetchEnabled = true;
    }
}