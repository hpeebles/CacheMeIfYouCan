using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFunc;
        internal int MaxFetchBatchSize { get; private set; }
        internal Func<IEqualityComparer<TK>, int, IDictionary<TK, TV>> DictionaryFactoryFunc { get; private set; }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;
        }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof(TK), typeof(TV)))
        {
            _inputFunc = inputFunc;
        }
        
        public TConfig WithBatchedFetches(int batchSize)
        {
            MaxFetchBatchSize = batchSize;
            return (TConfig)this;
        }

        public TConfig WithReturnDictionaryFactory(
            Func<IEqualityComparer<TK>, int, IDictionary<TK, TV>> dictionaryFactoryFunc)
        {
            DictionaryFactoryFunc = dictionaryFactoryFunc;
            return (TConfig)this;
        }
        
        internal EnumerableKeyFunctionCache<TK, TV> BuildEnumerableKeyFunctionCache()
        {
            var keyComparer = GetKeyComparer();
            
            var cache = BuildCache(keyComparer);
            
            var keySerializer = GetKeySerializer();

            if (KeysToRemoveObservable != null)
            {
                KeysToRemoveObservable
                    .SelectMany(k => Observable.FromAsync(() => cache.Remove(new Key<TK>(k, keySerializer)).AsTask()))
                    .Retry()
                    .Subscribe();
            }
            
            var functionCache = new EnumerableKeyFunctionCache<TK, TV>(
                _inputFunc,
                FunctionName,
                cache,
                TimeToLive ?? DefaultSettings.Cache.TimeToLive,
                keySerializer,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                keyComparer,
                MaxFetchBatchSize);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
    }
}