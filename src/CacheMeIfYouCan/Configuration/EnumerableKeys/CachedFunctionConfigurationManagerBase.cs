using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;
using CacheMeIfYouCan.Internal.RequestConverters;
using CacheMeIfYouCan.Internal.ResponseConverters;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerBase<TParams, TRequest, TResponse, TKey, TValue, TConfig>
        where TConfig : CachedFunctionConfigurationManagerBase<TParams, TRequest, TResponse, TKey, TValue, TConfig>
        where TRequest : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue> _config =
            new CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue>();
        
        private Func<ReadOnlyMemory<TKey>, TRequest> _requestConverter;
        private Func<Dictionary<TKey, TValue>, TResponse> _responseConverter;

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            _config.TimeToLive = timeToLive;
            return (TConfig)this;
        }

        private protected TConfig WithTimeToLiveFactoryInternal(Func<TParams, ReadOnlyMemory<TKey>, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = timeToLiveFactory;
            return (TConfig)this;
        }
        
        public TConfig WithLocalCache(ILocalCache<TKey, TValue> cache)
        {
            _config.LocalCache = cache;
            return (TConfig)this;
        }

        public TConfig WithDistributedCache(IDistributedCache<TKey, TValue> cache)
        {
            _config.DistributedCache = cache;
            return (TConfig)this;
        }

        public TConfig WithMemoryCache(Func<TKey, string> keySerializer = null)
        {
            return WithLocalCache(new MemoryCache<TKey, TValue>(keySerializer));
        }

        public TConfig WithDictionaryCache(IEqualityComparer<TKey> keyComparer = null)
        {
            return WithLocalCache(new DictionaryCache<TKey, TValue>(keyComparer));
        }

        public TConfig DisableCaching(bool disableCaching = true)
        {
            _config.DisableCaching = disableCaching;
            return (TConfig)this;
        }

        public TConfig FillMissingKeys(TValue value)
        {
            _config.FillMissingKeysConstantValue = (true, value);
            return (TConfig)this;
        }

        public TConfig FillMissingKeys(Func<TKey, TValue> valueFactory)
        {
            _config.FillMissingKeysValueFactory = valueFactory;
            return (TConfig)this;
        }

        public TConfig FilterResponseToWhere(Func<TKey, TValue, bool> predicate)
        {
            _config.FilterResponsePredicate = predicate;
            return (TConfig)this;
        }

        private protected TConfig DontGetFromCacheWhenInternal(Func<TParams, bool> predicate)
        {
            _config.SkipCacheGetOuterPredicate = _config.SkipCacheGetOuterPredicate.Or(predicate);
            return (TConfig)this;
        }
        
        private protected TConfig DontGetFromCacheWhenInternal(Func<TParams, TKey, bool> predicate)
        {
            _config.SkipCacheGetInnerPredicate = _config.SkipCacheGetInnerPredicate.Or(predicate);
            return (TConfig)this;
        }

        private protected TConfig DontStoreInCacheWhenInternal(Func<TParams, bool> predicate)
        {
            _config.SkipCacheSetOuterPredicate = _config.SkipCacheGetOuterPredicate.Or(predicate);
            return (TConfig)this;
        }

        private protected TConfig DontStoreInCacheWhenInternal(Func<TParams, TKey, TValue, bool> predicate)
        {
            _config.SkipCacheSetInnerPredicate = _config.SkipCacheSetInnerPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig DontGetFromLocalCacheWhen(Func<TKey, bool> predicate)
        {
            _config.SkipLocalCacheGetPredicate = _config.SkipLocalCacheGetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig DontStoreInLocalCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }
        
        public TConfig DontGetFromDistributedCacheWhen(Func<TKey, bool> predicate)
        {
            _config.SkipDistributedCacheGetPredicate = _config.SkipDistributedCacheGetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig DontStoreInDistributedCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig WithBatchedFetches(int maxBatchSize, BatchBehaviour batchBehaviour = BatchBehaviour.FillBatchesEvenly)
        {
            if (maxBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize));
            
            _config.MaxBatchSize = maxBatchSize;
            _config.BatchBehaviour = batchBehaviour;
            return (TConfig)this;
        }

        public TConfig OnResult(
            Action<SuccessfulRequestEvent<TParams, TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent<TParams, TKey>> onException = null)
        {
            if (!(onSuccess is null))
                _config.OnSuccessAction += onSuccess;
            
            if (!(onException is null))
                _config.OnExceptionAction += onException;

            return (TConfig)this;
        }

        public TConfig WithRequestConverter(Func<ReadOnlyMemory<TKey>, TRequest> requestConverter)
        {
            _requestConverter = requestConverter;
            return (TConfig)this;
        }

        public TConfig WithResponseConverter(Func<Dictionary<TKey, TValue>, TResponse> responseConverter)
        {
            _responseConverter = responseConverter;
            return (TConfig)this;
        }

        private protected Func<ReadOnlyMemory<TKey>, TRequest> GetRequestConverter()
        {
            return _requestConverter ?? DefaultRequestConverterResolver.Get<TKey, TRequest>(_config.KeyComparer);
        }
        
        private protected Func<Dictionary<TKey, TValue>, TResponse> GetResponseConverter()
        {
            return _responseConverter ?? DefaultResponseConverterResolver.Get<TKey, TValue, TResponse>(_config.KeyComparer);
        }

        private protected CachedFunctionWithEnumerableKeys<TParams, TKey, TValue> BuildCachedFunction(
            Func<TParams, ReadOnlyMemory<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction)
        {
            return new CachedFunctionWithEnumerableKeys<TParams, TKey, TValue>(originalFunction, _config);
        }
    }
}