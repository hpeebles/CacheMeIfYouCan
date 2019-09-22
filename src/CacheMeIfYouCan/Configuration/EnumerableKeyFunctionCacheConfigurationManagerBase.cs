﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.FunctionCaches;
using CacheMeIfYouCan.Serializers;

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
        internal Func<TK, TV> MissingKeyValueFactory { get; private set; }

        internal EnumerableKeyFunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, CancellationToken, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName)
            : base(functionName)
        {
            _inputFunc = inputFunc;

            TimeToLiveFactory = DefaultSettings.Cache.TimeToLiveFactory;

            if (DefaultSettings.Cache.ShouldFillMissingKeysWithDefaultValues)
                FillMissingKeys();
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
            
            if (interfaceConfig.FillMissingKeysWithDefaultValues)
                FillMissingKeys();
        }

        public TConfig WithTimeToLive(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            TimeToLiveFactory = () => timeToLive;

            if (jitterPercentage > 0)
                TimeToLiveFactory = TimeToLiveFactory.WithJitter(jitterPercentage);
            
            return (TConfig)this;
        }

        public TConfig WithKeySerializer(ISerializer<TK> serializer)
        {
            return base.WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public TConfig WithKeySerializer(Func<TK, string> serializer, Func<string, TK> deserializer = null)
        {
            return base.WithKeySerializer(serializer, deserializer);
        }

        public TConfig WithBatchedFetches(int batchSize, BatchBehaviour behaviour = BatchBehaviour.FillBatchesEvenly)
        {
            MaxFetchBatchSize = batchSize;
            BatchBehaviour = behaviour;
            return (TConfig)this;
        }
        
        public TConfig FillMissingKeys(TV value = default)
        {
            return FillMissingKeys(k => value);
        }

        public TConfig FillMissingKeys(Func<TK, TV> valueFactory)
        {
            MissingKeyValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            return (TConfig)this;
        }
        
        public new TConfig SkipCacheWhen(Func<TK, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            return base.SkipCacheWhen(predicate, settings);
        }

        public new TConfig OnlyStoreInLocalCacheWhen(Func<TK, TV, bool> predicate)
        {
            return base.OnlyStoreInLocalCacheWhen(predicate);
        }
        
        public new TConfig ReturnDefaultOnException(Func<TK, TV> defaultValueFactory)
        {
            return base.ReturnDefaultOnException(defaultValueFactory);
        }
        
        public new TConfig WithKeysToRemoveObservable(IObservable<TK> keysToRemoveObservable, bool removeFromLocalOnly = false)
        {
            return base.WithKeysToRemoveObservable(keysToRemoveObservable, removeFromLocalOnly);
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
                MissingKeyValueFactory);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }

        internal override KeyComparer<TK> GetKeyComparer()
        {
            return KeyComparerResolver.Get<TK>(KeyComparers);
        }
    }
}