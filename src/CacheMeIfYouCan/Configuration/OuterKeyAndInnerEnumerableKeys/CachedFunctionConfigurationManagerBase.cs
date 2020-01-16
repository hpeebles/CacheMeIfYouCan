using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;
using CacheMeIfYouCan.Internal.RequestConverters;
using CacheMeIfYouCan.Internal.ResponseConverters;

namespace CacheMeIfYouCan.Configuration.OuterKeyAndInnerEnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerBase<TOuterKey, TInnerKey, TValue, TInnerRequest, TResponse, TConfig>
        where TConfig : CachedFunctionConfigurationManagerBase<TOuterKey, TInnerKey, TValue, TInnerRequest, TResponse, TConfig>
        where TInnerRequest : IEnumerable<TInnerKey>
        where TResponse : IEnumerable<KeyValuePair<TInnerKey, TValue>>
    {
        private readonly CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue> _config =
            new CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue>();

        private Func<IReadOnlyCollection<TInnerKey>, TInnerRequest> _requestConverter;
        private Func<Dictionary<TInnerKey, TValue>, TResponse> _responseConverter;

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            return WithTimeToLiveFactory((_, __) => timeToLive);
        }
        
        public TConfig WithTimeToLiveFactory(Func<TOuterKey, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = (outerKey, _) => timeToLiveFactory(outerKey);
            return (TConfig)this;
        }

        public TConfig WithTimeToLiveFactory(Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = timeToLiveFactory;
            return (TConfig)this;
        }
        
        public TConfig WithLocalCache(ILocalCache<TOuterKey, TInnerKey, TValue> cache)
        {
            _config.LocalCache = cache;
            return (TConfig)this;
        }

        public TConfig WithDistributedCache(IDistributedCache<TOuterKey, TInnerKey, TValue> cache)
        {
            _config.DistributedCache = cache;
            return (TConfig)this;
        }

        public TConfig DisableCaching(bool disableCaching = true)
        {
            _config.DisableCaching = disableCaching;
            return (TConfig)this;
        }

        public TConfig SkipCacheWhen(
            Func<TOuterKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipCacheGetPredicateOuterKeyOnly = _config.SkipCacheGetPredicateOuterKeyOnly.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipCacheSetPredicateOuterKeyOnly = _config.SkipCacheSetPredicateOuterKeyOnly.Or(predicate);
            
            return (TConfig)this;
        }
        
        public TConfig SkipCacheWhen(
            Func<TOuterKey, TInnerKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipCacheGetPredicate = _config.SkipCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or((outerKey, innerKey, value) => predicate(outerKey, innerKey));

            return (TConfig)this;
        }

        public TConfig SkipCacheWhen(Func<TOuterKey, TInnerKey, TValue, bool> predicate)
        {
            _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(
            Func<TOuterKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipLocalCacheGetPredicateOuterKeyOnly = _config.SkipLocalCacheGetPredicateOuterKeyOnly.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipLocalCacheSetPredicateOuterKeyOnly = _config.SkipLocalCacheSetPredicateOuterKeyOnly.Or(predicate);

            return (TConfig)this;
        }
        
        public TConfig SkipLocalCacheWhen(
            Func<TOuterKey, TInnerKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipLocalCacheGetPredicate = _config.SkipLocalCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or((outerKey, innerKey, value) => predicate(outerKey, innerKey));

            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(Func<TOuterKey, TInnerKey, TValue, bool> predicate)
        {
            _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCacheWhen(
            Func<TOuterKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipDistributedCacheGetPredicateOuterKeyOnly = _config.SkipDistributedCacheGetPredicateOuterKeyOnly.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipDistributedCacheSetPredicateOuterKeyOnly = _config.SkipDistributedCacheSetPredicateOuterKeyOnly.Or(predicate);

            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCacheWhen(
            Func<TOuterKey, TInnerKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipDistributedCacheGetPredicate = _config.SkipDistributedCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or((outerKey, innerKey, value) => predicate(outerKey, innerKey));

            return (TConfig)this;
        }

        public TConfig SkipDistributedCacheWhen(Func<TOuterKey, TInnerKey, TValue, bool> predicate)
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

        public TConfig WithRequestConverter(Func<IReadOnlyCollection<TInnerKey>, TInnerRequest> requestConverter)
        {
            _requestConverter = requestConverter;
            return (TConfig)this;
        }

        public TConfig WithResponseConverter(Func<Dictionary<TInnerKey, TValue>, TResponse> responseConverter)
        {
            _responseConverter = responseConverter;
            return (TConfig)this;
        }
        
        private protected Func<IReadOnlyCollection<TInnerKey>, TInnerRequest> GetRequestConverter()
        {
            return _requestConverter ?? DefaultRequestConverterResolver.Get<TInnerKey, TInnerRequest>(_config.KeyComparer);
        }
        
        private protected Func<Dictionary<TInnerKey, TValue>, TResponse> GetResponseConverter()
        {
            return _responseConverter ?? DefaultResponseConverterResolver.Get<TInnerKey, TValue, TResponse>(_config.KeyComparer);
        }

        private protected CachedFunctionWithOuterKeyAndInnerEnumerableKeys<TOuterKey, TInnerKey, TValue> BuildCachedFunction(
            Func<TOuterKey, IReadOnlyCollection<TInnerKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TInnerKey, TValue>>>> originalFunction)
        {
            return new CachedFunctionWithOuterKeyAndInnerEnumerableKeys<TOuterKey, TInnerKey, TValue>(
                originalFunction,
                _config);
        }
    }
}