using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> _inputFunc;
        internal Func<TimeSpan> TimeToLiveFactory { get; private set; }
        internal int MaxFetchBatchSize { get; private set; }
        internal BatchBehaviour BatchBehaviour { get; private set; }
        internal Func<TK, TV> NegativeCachingValueFactory { get; private set; }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;

            TimeToLiveFactory = DefaultSettings.Cache.TimeToLiveFactory;

            if (DefaultSettings.Cache.NegativeCachingEnabled)
                WithNegativeCaching();
            
            if (DefaultSettings.Cache.ShouldOnlyStoreNegativesInLocalCache)
                OnlyStoreNegativesInLocalCache();
        }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        {
            _inputFunc = inputFunc;
            
            TimeToLiveFactory = interfaceConfig.TimeToLiveFactory ?? DefaultSettings.Cache.TimeToLiveFactory;

            if (interfaceConfig.MaxFetchBatchSize > 0)
                WithBatchedFetches(interfaceConfig.MaxFetchBatchSize, interfaceConfig.BatchBehaviour);
            else if (DefaultSettings.Cache.MaxFetchBatchSize > 0)
                WithBatchedFetches(DefaultSettings.Cache.MaxFetchBatchSize, DefaultSettings.Cache.BatchBehaviour);
            
            if (interfaceConfig.NegativeCachingEnabled)
                WithNegativeCaching();

            if (interfaceConfig.OnlyStoreNegativesInLocalCache)
                OnlyStoreNegativesInLocalCache();
        }

        public TConfig WithTimeToLive(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            TimeToLiveFactory = () => timeToLive;

            if (jitterPercentage > 0)
                TimeToLiveFactory = TimeToLiveFactory.WithJitter(jitterPercentage);
            
            return (TConfig)this;
        }
        
        public TConfig WithBatchedFetches(int batchSize, BatchBehaviour behaviour = BatchBehaviour.FillBatchesEvenly)
        {
            MaxFetchBatchSize = batchSize;
            BatchBehaviour = behaviour;
            return (TConfig)this;
        }

        public TConfig WithNegativeCaching(TV value = default)
        {
            return WithNegativeCaching(k => value);
        }

        public TConfig WithNegativeCaching(Func<TK, TV> valueFactory)
        {
            NegativeCachingValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            return (TConfig)this;
        }
        
        /// <summary>
        /// If set to true, only keys which have no corresponding values will be stored in the local cache. This can
        /// improve performance without using up much memory as the keys stored locally all have small values (null or
        /// default) with all values still stored in the distributed cache. This setting is ignored if not using a 2
        /// tier caching strategy
        /// </summary>
        public TConfig OnlyStoreNegativesInLocalCache(bool onlyStoreNegativesInLocalCache = true)
        {
            return typeof(TV).IsClass
                ? OnlyStoreInLocalCacheWhen((k, v) => v == null)
                : OnlyStoreInLocalCacheWhen((k, v) => v.Equals(default(TV)));
        }
        
        internal EnumerableKeyFunctionCache<TK, TV> BuildEnumerableKeyFunctionCache()
        {
            var keySerializer = GetKeySerializer();
            var keyComparer = GetKeyComparer();
            
            var cache = BuildCache(keySerializer, keyComparer);
            
            var functionCache = new EnumerableKeyFunctionCache<TK, TV>(
                _inputFunc,
                Name,
                cache,
                TimeToLiveFactory,
                DuplicateRequestCatchingEnabled ?? DefaultSettings.Cache.DuplicateRequestCatchingEnabled,
                keySerializer,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                keyComparer,
                MaxFetchBatchSize,
                BatchBehaviour,
                SkipCacheGetPredicate,
                SkipCacheSetPredicate,
                NegativeCachingValueFactory);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }

        internal override KeyComparer<TK> GetKeyComparer()
        {
            return KeyComparerResolver.Get<TK>(KeyComparers);
        }
    }
}