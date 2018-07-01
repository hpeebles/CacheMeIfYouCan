using System;

namespace CacheMeIfYouCan
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.FromDays(1);
        public static int MemoryCacheMaxSizeMB = 1024;
        public static bool EarlyFetchEnabled = true;
        public static ICacheFactory CacheFactory;
        public static readonly Serializers Serializers = new Serializers();
    }
}