using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    internal class CachedProxyConfig
    {
        public readonly Type InterfaceType;
        public readonly KeySerializers KeySerializers;
        public readonly ValueSerializers ValueSerializers;
        public readonly TimeSpan TimeToLive;
        public readonly int MemoryCacheMaxSizeMB;
        public readonly bool EarlyFetchEnabled;
        public readonly ICacheFactory CacheFactory;
        public readonly Action<FunctionCacheGetResult> OnResult;
        public readonly Action<FunctionCacheFetchResult> OnFetch;
        public readonly Action<FunctionCacheErrorEvent> OnError;
        public readonly IDictionary<MethodInfoKey, object> FunctionCacheConfigActions;
        
        public CachedProxyConfig(
            Type interfaceType,
            KeySerializers keySerializers,
            ValueSerializers valueSerializers,
            TimeSpan timeToLive,
            int memoryCacheMaxSizeMb,
            bool earlyFetchEnabled,
            ICacheFactory cacheFactory,
            Action<FunctionCacheGetResult> onResult,
            Action<FunctionCacheFetchResult> onFetch,
            Action<FunctionCacheErrorEvent> onError,
            IDictionary<MethodInfoKey, object> functionCacheConfigActions)
        {
            InterfaceType = interfaceType;
            KeySerializers = keySerializers;
            ValueSerializers = valueSerializers;
            TimeToLive = timeToLive;
            MemoryCacheMaxSizeMB = memoryCacheMaxSizeMb;
            EarlyFetchEnabled = earlyFetchEnabled;
            CacheFactory = cacheFactory;
            OnResult = onResult;
            OnFetch = onFetch;
            OnError = onError;
            FunctionCacheConfigActions = functionCacheConfigActions;
        }
    }
}