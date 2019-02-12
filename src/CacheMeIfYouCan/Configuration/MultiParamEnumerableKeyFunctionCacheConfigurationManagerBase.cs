using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK1, TK2, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, (TK1, TK2), TV>
        where TConfig : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK1, TK2, TV>
    {
        private readonly Func<TK1, IEnumerable<TK2>, Task<IDictionary<TK2, TV>>> _inputFunc;
        internal string KeyParamSeparator { get; private set; }
        internal int MaxFetchBatchSize { get; private set; }
        internal Func<IEqualityComparer<TK2>, int, IDictionary<TK2, TV>> DictionaryFactoryFunc { get; private set; }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<TK1, IEnumerable<TK2>, Task<IDictionary<TK2, TV>>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;
        }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<TK1, IEnumerable<TK2>, Task<IDictionary<TK2, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof((TK1, TK2)), typeof(TV)))
        {
            _inputFunc = inputFunc;
            MaxFetchBatchSize = interfaceConfig.MaxFetchBatchSize;
        }
        
        public TConfig WithKeyParamSeparator(string separator)
        {
            KeyParamSeparator = separator;
            return (TConfig)this;
        }
        
        public TConfig WithBatchedFetches(int batchSize)
        {
            MaxFetchBatchSize = batchSize;
            return (TConfig)this;
        }

        public TConfig WithReturnDictionaryFactory(
            Func<IEqualityComparer<TK2>, int, IDictionary<TK2, TV>> dictionaryFactoryFunc)
        {
            DictionaryFactoryFunc = dictionaryFactoryFunc;
            return (TConfig)this;
        }

        internal MultiParamEnumerableKeyFunctionCache<TK1, TK2, TV> BuildMultiParamEnumerableKeyFunctionCache(
            KeyComparer<TK1> keyComparer1 = null,
            Func<TK1, string> keySerializer1 = null)
        {
            if (keyComparer1 == null)
                keyComparer1 = KeyComparerResolver.Get<TK1>(KeyComparers);

            if (keySerializer1 == null)
                keySerializer1 = GetKeySerializerImpl<TK1>();
            
            var keyComparer2 = KeyComparerResolver.Get<TK2>(KeyComparers);
            
            var combinedKeyComparer = new KeyComparer<(TK1, TK2)>(
                new ValueTupleComparer<TK1, TK2>(keyComparer1.Inner, keyComparer2.Inner));
            
            var cache = BuildCache(GetKeySerializer(), combinedKeyComparer);

            var timeToLive = TimeToLive ?? DefaultSettings.Cache.TimeToLive;

            var keySerializer2 = GetKeySerializerImpl<TK2>();
            
            var functionCache = new MultiParamEnumerableKeyFunctionCache<TK1, TK2, TV>(
                _inputFunc,
                FunctionName,
                cache,
                timeToLive,
                keySerializer1,
                keySerializer2,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                keyComparer1,
                keyComparer2,
                KeyParamSeparator,
                MaxFetchBatchSize,
                SkipCacheGetPredicate,
                SkipCacheSetPredicate);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
    }
}