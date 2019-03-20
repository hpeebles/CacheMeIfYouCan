using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK1, TK2, TV>
        : FunctionCacheConfigurationManagerBase<TConfig, (TK1, TK2), TV>
        where TConfig : MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase<TConfig, TK1, TK2, TV>
    {
        private readonly Func<TK1, IEnumerable<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> _inputFunc;
        internal Func<TK1, TimeSpan> TimeToLiveFactory { get; private set; }
        internal string KeyParamSeparator { get; private set; }
        internal Func<TK1, int> MaxFetchBatchSizeFunc { get; private set; }
        internal BatchBehaviour BatchBehaviour { get; private set; }
        internal Func<(TK1, TK2), TV> NegativeCachingValueFactory { get; private set; }
        internal int[] ParametersToExcludeFromKey { get; private set; }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<TK1, IEnumerable<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;
            
            var timeToLiveFactory = DefaultSettings.Cache.TimeToLiveFactory;
            if (timeToLiveFactory != null)
                TimeToLiveFactory = k => timeToLiveFactory();
        }

        internal MultiParamEnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<TK1, IEnumerable<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> inputFunc,
            CachedProxyConfig interfaceConfig,
            MethodInfo methodInfo)
            : base(
                $"{interfaceConfig.InterfaceType.Name}.{methodInfo.Name}",
                interfaceConfig,
                new CachedProxyFunctionInfo(interfaceConfig.InterfaceType, methodInfo, typeof((TK1, TK2)), typeof(TV)))
        {
            _inputFunc = inputFunc;
            
            var timeToLiveFactory = interfaceConfig.TimeToLiveFactory ?? DefaultSettings.Cache.TimeToLiveFactory;
            if (timeToLiveFactory != null)
                TimeToLiveFactory = k => timeToLiveFactory();
            
            KeyParamSeparator = interfaceConfig.KeyParamSeparator;

            if (interfaceConfig.MaxFetchBatchSize > 0)
                WithBatchedFetches(interfaceConfig.MaxFetchBatchSize, interfaceConfig.BatchBehaviour);
            else if (DefaultSettings.Cache.MaxFetchBatchSize > 0)
                WithBatchedFetches(DefaultSettings.Cache.MaxFetchBatchSize, DefaultSettings.Cache.BatchBehaviour);
        }
        
        public TConfig WithTimeToLive(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            return WithTimeToLiveFactory(k => timeToLive, jitterPercentage);
        }

        public TConfig WithTimeToLiveFactory(Func<TK1, TimeSpan> timeToLiveFactory, double jitterPercentage = 0)
        {
            TimeToLiveFactory = timeToLiveFactory ?? throw new ArgumentNullException(nameof(timeToLiveFactory));

            if (jitterPercentage > 0)
                TimeToLiveFactory = TimeToLiveFactory.WithJitter(jitterPercentage);
            
            return (TConfig)this;
        }
        
        public TConfig WithKeyParamSeparator(string separator)
        {
            if (String.IsNullOrEmpty(separator))
                throw new ArgumentException(nameof(separator));

            KeyParamSeparator = separator;
            return (TConfig)this;
        }
        
        public TConfig WithBatchedFetches(int batchSize, BatchBehaviour behaviour = BatchBehaviour.FillBatchesEvenly)
        {
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize));

            return WithBatchedFetches(k => batchSize, behaviour);
        }

        public TConfig WithBatchedFetches(Func<TK1, int> batchSizeFunc, BatchBehaviour behaviour = BatchBehaviour.FillBatchesEvenly)
        {
            MaxFetchBatchSizeFunc = batchSizeFunc;
            BatchBehaviour = behaviour;
            return (TConfig)this;
        }

        public TConfig WithNegativeCaching(TV value = default)
        {
            return WithNegativeCaching(k => value);
        }

        public TConfig WithNegativeCaching(Func<(TK1, TK2), TV> valueFactory)
        {
            NegativeCachingValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            return (TConfig)this;
        }
        
        protected TConfig ExcludeParametersFromKeyImpl(int[] parameterIndexes, int totalParameterCount)
        {
            parameterIndexes = parameterIndexes.Distinct().ToArray();
            
            foreach (var index in parameterIndexes)
            {
                if (0 < index || index >= totalParameterCount - 1)
                    throw new ArgumentOutOfRangeException(nameof(parameterIndexes), $"Index '{index}' is not valid");
            }
            
            if (parameterIndexes.Length >= totalParameterCount)
                throw new ArgumentException("You cannot exclude all parameters from the key");

            
            ParametersToExcludeFromKey = parameterIndexes;
            return (TConfig)this;
        }

        internal MultiParamEnumerableKeyFunctionCache<TK1, TK2, TV> BuildMultiParamEnumerableKeyFunctionCache(
            KeyComparer<TK1> key1Comparer,
            Func<TK1, string> key1Serializer)
        {
            var key2Comparer = KeyComparerResolver.Get<TK2>(KeyComparers);
            
            var combinedKeyComparer = new KeyComparer<(TK1, TK2)>(
                new ValueTupleComparer<TK1, TK2>(key1Comparer.Inner, key2Comparer.Inner));
            
            var cache = BuildCache(GetKeySerializer(), combinedKeyComparer);

            var key2Serializer = GetKeySerializerImpl<TK2>();
            
            var functionCache = new MultiParamEnumerableKeyFunctionCache<TK1, TK2, TV>(
                _inputFunc,
                Name,
                cache,
                TimeToLiveFactory,
                DuplicateRequestCatchingEnabled ?? DefaultSettings.Cache.DuplicateRequestCatchingEnabled,
                key1Serializer,
                key2Serializer,
                DefaultValueFactory,
                OnResultAction,
                OnFetchAction,
                OnExceptionAction,
                key1Comparer,
                key2Comparer,
                KeyParamSeparator ?? DefaultSettings.Cache.KeyParamSeparator,
                MaxFetchBatchSizeFunc,
                BatchBehaviour,
                SkipCacheGetPredicate,
                SkipCacheSetPredicate,
                NegativeCachingValueFactory);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
    }
}