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
    public abstract class SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<TK, CancellationToken, Task<TV>> _inputFunc;
        private Func<TK, TV, TimeSpan> _timeToLiveFactory;

        internal SingleKeyFunctionCacheConfigurationManagerBase(Func<TK, CancellationToken, Task<TV>> inputFunc, string name)
            : base(name)
        {
            _inputFunc = inputFunc;
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
        }

        public override TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            TimeToLive = timeToLive;
            _timeToLiveFactory = null;
            return (TConfig)this;
        }
        
        public TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            _timeToLiveFactory = timeToLiveFactory;
            TimeToLive = null;
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

            Func<TK, TV, TimeSpan> timeToLiveFactory;
            if (_timeToLiveFactory != null)
            {
                timeToLiveFactory = _timeToLiveFactory;
            }
            else
            {
                var timeToLive = TimeToLive ?? DefaultSettings.Cache.TimeToLive;
                timeToLiveFactory = (k, v) => timeToLive;
            }

            var functionCache = new SingleKeyFunctionCache<TK, TV>(
                _inputFunc,
                Name,
                cache,
                timeToLiveFactory,
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
    }
}