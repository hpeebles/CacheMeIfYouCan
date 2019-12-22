using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;
using CacheMeIfYouCan.Internal.RequestConverters;
using CacheMeIfYouCan.Internal.ResponseConverters;

namespace CacheMeIfYouCan.Configuration.EnumerableKeys
{
    public abstract class CachedFunctionConfigurationManagerBase<TKey, TValue, TRequest, TResponse, TConfig>
        where TConfig : CachedFunctionConfigurationManagerBase<TKey, TValue, TRequest, TResponse, TConfig>
        where TRequest : IEnumerable<TKey>
        where TResponse : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly CachedFunctionWithEnumerableKeysConfiguration<TKey, TValue> _config =
            new CachedFunctionWithEnumerableKeysConfiguration<TKey, TValue>();
        
        private Func<IReadOnlyCollection<TKey>, TRequest> _requestConverter;
        private Func<Dictionary<TKey, TValue>, TResponse> _responseConverter;

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            return WithTimeToLiveFactory(_ => timeToLive);
        }

        public TConfig WithTimeToLiveFactory(Func<IReadOnlyCollection<TKey>, TimeSpan> timeToLiveFactory)
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
            return _requestConverter ?? GetDefaultRequestConverter();

            Func<IReadOnlyCollection<TKey>, TRequest> GetDefaultRequestConverter()
            {
                IRequestConverter<TKey, TRequest> converter;
                if (typeof(TRequest).IsAssignableFrom(typeof(IReadOnlyCollection<TKey>)))
                    converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new IReadOnlyCollectionConverter<TKey>());
                else if (typeof(TRequest).IsAssignableFrom(typeof(List<TKey>)))
                    converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new ListConverter<TKey>());
                else if (typeof(TRequest).IsAssignableFrom(typeof(TKey[])))
                    converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new ArrayConverter<TKey>());
                else if (typeof(TRequest).IsAssignableFrom(typeof(HashSet<TKey>)))
                    converter = Unsafe.As<IRequestConverter<TKey, TRequest>>(new HashSetConverter<TKey>(_config.KeyComparer));
                else
                    throw new Exception($"No request converter defined for type '{typeof(TRequest)}'");

                return converter.Convert;
            }
        }
        
        private protected Func<Dictionary<TKey, TValue>, TResponse> GetResponseConverter()
        {
            return _responseConverter ?? GetDefaultResponseConverter();

            Func<Dictionary<TKey, TValue>, TResponse> GetDefaultResponseConverter()
            {
                IResponseConverter<TKey, TValue, TResponse> converter;
                if (typeof(TResponse).IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
                    converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new DictionaryConverter<TKey, TValue>());
                else if (typeof(TResponse).IsAssignableFrom(typeof(ConcurrentDictionary<TKey, TValue>)))
                    converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ConcurrentDictionaryConverter<TKey, TValue>());
                else if (typeof(TResponse).IsAssignableFrom(typeof(List<KeyValuePair<TKey, TValue>>)))
                    converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ListKeyValuePairConverter<TKey, TValue>());
                else if (typeof(TResponse).IsAssignableFrom(typeof(KeyValuePair<TKey, TValue>[])))
                    converter = Unsafe.As<IResponseConverter<TKey, TValue, TResponse>>(new ArrayKeyValuePairConverter<TKey, TValue>());
                else
                    throw new Exception($"No response converter defined for type '{typeof(TResponse)}'");

                return converter.Convert;
            }
        }

        private protected CachedFunctionWithEnumerableKeys<TKey, TValue> BuildCachedFunction(
            Func<IReadOnlyCollection<TKey>, CancellationToken, Task<IEnumerable<KeyValuePair<TKey, TValue>>>> originalFunction)
        {
            return new CachedFunctionWithEnumerableKeys<TKey, TValue>(originalFunction, _config);
        }
    }
}