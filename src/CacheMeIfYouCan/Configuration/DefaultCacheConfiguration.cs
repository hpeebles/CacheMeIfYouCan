using System;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCacheConfiguration
    {
        public TimeSpan TimeToLive = TimeSpan.FromHours(1);
        public bool EarlyFetchEnabled = true;
        public bool DisableCache;
        public ILocalCacheFactory LocalCacheFactory;
        public ICacheFactory RemoteCacheFactory;
        public Action<FunctionCacheGetResult> OnResult;
        public Action<FunctionCacheFetchResult> OnFetch;
        public Action<FunctionCacheErrorEvent> OnError;
        public Action<CacheGetResult> OnCacheGet;
        public Action<CacheSetResult> OnCacheSet;
        public readonly KeySerializers KeySerializers = new KeySerializers();
        public readonly ValueSerializers ValueSerializers = new ValueSerializers();
    }
    
    public static class DefaultCacheConfig
    {
        public static DefaultCacheConfiguration Configuration = new DefaultCacheConfiguration();
    }
}