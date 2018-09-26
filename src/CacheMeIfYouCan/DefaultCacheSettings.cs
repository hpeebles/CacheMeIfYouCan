using System;

namespace CacheMeIfYouCan
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.FromHours(1);
        public static int MemoryCacheMaxSizeMB = 1024;
        public static bool EarlyFetchEnabled = true;
        public static ICacheFactory CacheFactory;
        public static Action<FunctionCacheGetResult> OnResult;
        public static Action<FunctionCacheFetchResult> OnFetch;
        public static Action<FunctionCacheErrorEvent> OnError;
        public static readonly KeySerializers KeySerializers = new KeySerializers();
        public static readonly ValueSerializers ValueSerializers = new ValueSerializers();
    }
}