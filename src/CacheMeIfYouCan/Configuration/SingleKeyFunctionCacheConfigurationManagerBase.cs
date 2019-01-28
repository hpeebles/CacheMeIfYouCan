using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : SingleKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<TK, Task<TV>> _inputFunc;
        private Func<TK, TV, TimeSpan> _timeToLiveFactory;

        internal SingleKeyFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;
        }

        internal SingleKeyFunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
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
            var keyComparer = GetKeyComparer();
            
            var cache = BuildCache(keyComparer);

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

            var keySerializer = GetKeySerializer();

            if (KeysToRemoveObservable != null)
            {
                KeysToRemoveObservable
                    .SelectMany(k => Observable.FromAsync(() => cache.Remove(new Key<TK>(k, keySerializer)).AsTask()))
                    .Retry()
                    .Subscribe();
            }
            
            var functionCache = new SingleKeyFunctionCache<TK, TV>(
                _inputFunc,
                FunctionName,
                cache,
                timeToLiveFactory,
                keySerializer,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                keyComparer);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
    }
}