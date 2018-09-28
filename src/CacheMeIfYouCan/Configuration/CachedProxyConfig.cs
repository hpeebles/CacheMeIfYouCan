using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    internal class CachedProxyConfig
    {
        public readonly Type InterfaceType;
        public readonly KeySerializers KeySerializers;
        public readonly ValueSerializers ValueSerializers;
        public readonly TimeSpan? TimeToLive;
        public readonly bool? EarlyFetchEnabled;
        public readonly bool? DisableCache;
        public readonly ILocalCacheFactory LocalCacheFactory;
        public readonly ICacheFactory RemoteCacheFactory;
        public readonly Action<FunctionCacheGetResult> OnResult;
        public readonly Action<FunctionCacheFetchResult> OnFetch;
        public readonly Action<FunctionCacheErrorEvent> OnError;
        public readonly IDictionary<MethodInfoKey, object> FunctionCacheConfigActions;
        
        public CachedProxyConfig(
            Type interfaceType,
            KeySerializers keySerializers,
            ValueSerializers valueSerializers,
            TimeSpan? timeToLive,
            bool? earlyFetchEnabled,
            bool? disableCache,
            ILocalCacheFactory localCacheFactory,
            ICacheFactory remoteCacheFactory,
            Action<FunctionCacheGetResult> onResult,
            Action<FunctionCacheFetchResult> onFetch,
            Action<FunctionCacheErrorEvent> onError,
            IDictionary<MethodInfoKey, object> functionCacheConfigActions)
        {
            InterfaceType = interfaceType;
            KeySerializers = keySerializers;
            ValueSerializers = valueSerializers;
            TimeToLive = timeToLive;
            EarlyFetchEnabled = earlyFetchEnabled;
            DisableCache = disableCache;
            LocalCacheFactory = localCacheFactory;
            RemoteCacheFactory = remoteCacheFactory;
            OnResult = onResult;
            OnFetch = onFetch;
            OnError = onError;
            FunctionCacheConfigActions = functionCacheConfigActions;
        }
    }
}