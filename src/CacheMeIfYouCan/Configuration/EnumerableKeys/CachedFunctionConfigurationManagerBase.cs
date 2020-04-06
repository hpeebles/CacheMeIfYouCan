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
        
        private Func<IReadOnlyCollection<TKey>, TRequest> _requestConverter;
        private Func<Dictionary<TKey, TValue>, TResponse> _responseConverter;

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            _config.TimeToLive = timeToLive;
            return (TConfig)this;
        }

        private protected TConfig WithTimeToLiveFactoryInternal(Func<TParams, IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
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
        
        public TConfig SkipCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipCacheGetPredicate = _config.SkipCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipLocalCacheGetPredicate = _config.SkipLocalCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipDistributedCacheGetPredicate = _config.SkipDistributedCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipDistributedCacheWhen(Func<TKey, TValue, bool> predicate)
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

        public TConfig WithRequestConverter(Func<IReadOnlyCollection<TKey>, TRequest> requestConverter)
        {
            _requestConverter = requestConverter;
            return (TConfig)this;
        }

        public TConfig WithResponseConverter(Func<Dictionary<TKey, TValue>, TResponse> responseConverter)
        {
            _responseConverter = responseConverter;
            return (TConfig)this;
        }

        private protected Func<IReadOnlyCollection<TKey>, TRequest> GetRequestConverter()
        {
            return _requestConverter ?? DefaultRequestConverterResolver.Get<TKey, TRequest>(_config.KeyComparer);
        }
        
        private protected Func<Dictionary<TKey, TValue>, TResponse> GetResponseConverter()
        {
            return _responseConverter ?? DefaultResponseConverterResolver.Get<TKey, TValue, TResponse>(_config.KeyComparer);
        }

        private protected CachedFunctionWithEnumerableKeys<TParams, TKey, TValue> BuildCachedFunction(
            Func<TParams, IReadOnlyCollection<TKey>, CancellationToken, ValueTask<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction)
        {
            return new CachedFunctionWithEnumerableKeys<TParams, TKey, TValue>(originalFunction, _config);
        }
    }
}