using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : EnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFunc;
        internal int MaxFetchBatchSize { get; private set; }
        internal Func<TK, TV> NegativeCachingValueFactory { get; private set; }

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
            MaxFetchBatchSize = interfaceConfig.MaxFetchBatchSize;
        }
        
        public TConfig WithBatchedFetches(int batchSize)
        {
            MaxFetchBatchSize = batchSize;
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
        
        internal EnumerableKeyFunctionCache<TK, TV> BuildEnumerableKeyFunctionCache()
        {
            var keySerializer = GetKeySerializer();
            var keyComparer = GetKeyComparer();
            
            var cache = BuildCache(keySerializer, keyComparer);
            
            var functionCache = new EnumerableKeyFunctionCache<TK, TV>(
                _inputFunc,
                Name,
                cache,
                TimeToLive ?? DefaultSettings.Cache.TimeToLive,
                keySerializer,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                keyComparer,
                MaxFetchBatchSize,
                SkipCacheGetPredicate,
                SkipCacheSetPredicate,
                NegativeCachingValueFactory);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
    }
}