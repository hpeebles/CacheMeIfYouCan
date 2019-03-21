using System;
using System.Collections.Generic;
using System.Reflection;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal
{
    internal class CachedProxyConfig
    {
        public CachedProxyConfig(
            Type interfaceType,
            KeySerializers keySerializers,
            ValueSerializers valueSerializers,
            EqualityComparers keyComparers,
            Func<MethodInfo, string> nameGenerator,
            Func<TimeSpan> timeToLiveFactory,
            Func<TimeSpan> localCacheTimeToLiveOverride,
            bool? disableCache,
            bool? catchDuplcateRequests,
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory,
            Func<MethodInfo, string> keyspacePrefixFunc,
            Action<FunctionCacheGetResult> onResult,
            Action<FunctionCacheFetchResult> onFetch,
            Action<FunctionCacheException> onException,
            Action<CacheGetResult> onCacheGet,
            Action<CacheSetResult> onCacheSet,
            Action<CacheRemoveResult> onCacheRemove,
            Action<CacheException> onCacheException,
            string keyParamSeparator,
            int maxFetchBatchSize,
            BatchBehaviour batchBehaviour,
            bool negativeCachingEnabled,
            bool onlyStoreNegativesInLocalCache,
            IDictionary<MethodInfoKey, object> functionCacheConfigActions)
        {
            InterfaceType = interfaceType;
            KeySerializers = keySerializers;
            ValueSerializers = valueSerializers;
            KeyComparers = keyComparers;
            NameGenerator = nameGenerator;
            TimeToLiveFactory = timeToLiveFactory;
            LocalCacheTimeToLiveOverride = localCacheTimeToLiveOverride;
            DisableCache = disableCache;
            CatchDuplicateRequests = catchDuplcateRequests;
            LocalCacheFactory = localCacheFactory;
            DistributedCacheFactory = distributedCacheFactory;
            KeyspacePrefixFunc = keyspacePrefixFunc;
            OnResult = onResult;
            OnFetch = onFetch;
            OnException = onException;
            OnCacheGet = onCacheGet;
            OnCacheSet = onCacheSet;
            OnCacheRemove = onCacheRemove;
            OnCacheException = onCacheException;
            KeyParamSeparator = keyParamSeparator;
            MaxFetchBatchSize = maxFetchBatchSize;
            BatchBehaviour = batchBehaviour;
            NegativeCachingEnabled = negativeCachingEnabled;
            OnlyStoreNegativesInLocalCache = onlyStoreNegativesInLocalCache;
            FunctionCacheConfigActions = functionCacheConfigActions;
        }
        
        public Type InterfaceType { get; }
        public KeySerializers KeySerializers { get; }
        public ValueSerializers ValueSerializers { get; }
        public EqualityComparers KeyComparers { get; }
        public Func<MethodInfo, string> NameGenerator { get; }
        public Func<TimeSpan> TimeToLiveFactory { get; }
        public Func<TimeSpan> LocalCacheTimeToLiveOverride { get; }
        public bool? DisableCache { get; }
        public bool? CatchDuplicateRequests { get; }
        public ILocalCacheFactory LocalCacheFactory { get; }
        public IDistributedCacheFactory DistributedCacheFactory { get; }
        public Func<MethodInfo, string> KeyspacePrefixFunc { get; }
        public Action<FunctionCacheGetResult> OnResult { get; }
        public Action<FunctionCacheFetchResult> OnFetch { get; }
        public Action<FunctionCacheException> OnException { get; }
        public Action<CacheGetResult> OnCacheGet { get; }
        public Action<CacheSetResult> OnCacheSet { get; }
        public Action<CacheRemoveResult> OnCacheRemove { get; }
        public Action<CacheException> OnCacheException { get; }
        public string KeyParamSeparator { get; }
        public int MaxFetchBatchSize { get; }
        public BatchBehaviour BatchBehaviour { get; }
        public bool NegativeCachingEnabled { get; }
        public bool OnlyStoreNegativesInLocalCache { get; }
        public IDictionary<MethodInfoKey, object> FunctionCacheConfigActions { get; }
    }
}