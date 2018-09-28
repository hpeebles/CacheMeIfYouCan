using System;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public static class DefaultCacheSettings
    {
        public static TimeSpan TimeToLive = TimeSpan.FromHours(1);
        public static bool EarlyFetchEnabled = true;
        public static bool DisableCache;
        public static ILocalCacheFactory LocalCacheFactory;
        public static ICacheFactory RemoteCacheFactory;
        public static Action<FunctionCacheGetResult> OnResult;
        public static Action<FunctionCacheFetchResult> OnFetch;
        public static Action<FunctionCacheErrorEvent> OnError;
        public static readonly KeySerializers KeySerializers = new KeySerializers();
        public static readonly ValueSerializers ValueSerializers = new ValueSerializers();
    }
}