using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal
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
        public readonly IDistributedCacheFactory DistributedCacheFactory;
        public readonly Func<CachedProxyFunctionInfo, string> KeyspacePrefixFunc;
        public readonly Action<FunctionCacheGetResult> OnResult;
        public readonly Action<FunctionCacheFetchResult> OnFetch;
        public readonly Action<FunctionCacheException> OnException;
        public readonly Action<CacheGetResult> OnCacheGet;
        public readonly Action<CacheSetResult> OnCacheSet;
        public readonly Action<CacheException> OnCacheException;
        public readonly IDictionary<MethodInfoKey, object> FunctionCacheConfigActions;
        
        public CachedProxyConfig(
            Type interfaceType,
            KeySerializers keySerializers,
            ValueSerializers valueSerializers,
            TimeSpan? timeToLive,
            bool? earlyFetchEnabled,
            bool? disableCache,
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory,
            Func<CachedProxyFunctionInfo, string> keyspacePrefixFunc,
            Action<FunctionCacheGetResult> onResult,
            Action<FunctionCacheFetchResult> onFetch,
            Action<FunctionCacheException> onException,
            Action<CacheGetResult> onCacheGet,
            Action<CacheSetResult> onCacheSet,
            Action<CacheException> onCacheException,
            IDictionary<MethodInfoKey, object> functionCacheConfigActions)
        {
            InterfaceType = interfaceType;
            KeySerializers = keySerializers;
            ValueSerializers = valueSerializers;
            TimeToLive = timeToLive;
            EarlyFetchEnabled = earlyFetchEnabled;
            DisableCache = disableCache;
            LocalCacheFactory = localCacheFactory;
            DistributedCacheFactory = distributedCacheFactory;
            KeyspacePrefixFunc = keyspacePrefixFunc;
            OnResult = onResult;
            OnFetch = onFetch;
            OnException = onException;
            OnCacheGet = onCacheGet;
            OnCacheSet = onCacheSet;
            OnCacheException = onCacheException;
            FunctionCacheConfigActions = functionCacheConfigActions;
        }
    }
}