using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<TK, CancellationToken, Task<TV>> _inputFunc;
        internal Func<TK, TV, TimeSpan> TimeToLiveFactory { get; private set; }

        internal SingleKeyFunctionCacheConfigurationManagerBase(Func<TK, CancellationToken, Task<TV>> inputFunc, string name)
            : base(name)
        {
            _inputFunc = inputFunc;
            
            var timeToLiveFactory = DefaultSettings.Cache.TimeToLiveFactory;
            if (timeToLiveFactory != null)
                TimeToLiveFactory = (k, v) => timeToLiveFactory();
        }

        internal SingleKeyFunctionCacheConfigurationManagerBase(
            Func<TK, CancellationToken, Task<TV>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        {
            _inputFunc = inputFunc;

            var timeToLiveFactory = interfaceConfig.TimeToLiveFactory ?? DefaultSettings.Cache.TimeToLiveFactory;
            if (timeToLiveFactory != null)
                TimeToLiveFactory = (k, v) => timeToLiveFactory();
        }
        
        public TConfig WithTimeToLive(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            return WithTimeToLiveFactory((k, v) => timeToLive, jitterPercentage);
        }
        
        protected TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            TimeToLiveFactory = timeToLiveFactory ?? throw new ArgumentNullException(nameof(timeToLiveFactory));

            if (jitterPercentage > 0)
                TimeToLiveFactory = TimeToLiveFactory.WithJitter(jitterPercentage);
            
            return (TConfig)this;
        }

        public TConfig WithKeyComparer(IEqualityComparer<TK> comparer)
        {
            return WithKeyComparerInternal(comparer);
        }
        
        internal SingleKeyFunctionCache<TK, TV> BuildFunctionCacheSingle()
        {
            var keySerializer = GetKeySerializer();
            var keyComparer = GetKeyComparer();
            
            var cache = BuildCache(keySerializer, keyComparer);

            var functionCache = new SingleKeyFunctionCache<TK, TV>(
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
                SkipCacheGetPredicate,
                SkipCacheSetPredicate);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }

        internal override KeyComparer<TK> GetKeyComparer()
        {
            return KeyComparerResolver.Get<TK>(KeyComparers);
        }
    }
}