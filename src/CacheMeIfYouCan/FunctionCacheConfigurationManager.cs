using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class FunctionCacheConfigurationManager<TK, TV>
    {
        private readonly Func<TK, Task<TV>> _inputFunc;
        private readonly string _cacheName;
        private readonly CacheConfigOverrides _config;
        private readonly IList<Action<FunctionCacheGetResult<TK, TV>>> _onResult;
        private readonly IList<Action<FunctionCacheFetchResult<TK, TV>>> _onFetch;
        private readonly IList<Action<FunctionCacheErrorEvent<TK>>> _onError;
        private readonly object _lock;
        private Func<TK, string> _keySerializer;
        private Func<ICache<TV>> _cacheFactoryFunc;
        private Func<TV> _defaultValueFactory;
        private Func<TK, Task<TV>> _cachedFunc;
        
        internal FunctionCacheConfigurationManager(Func<TK, Task<TV>> inputFunc, string cacheName)
        {
            _inputFunc = inputFunc;
            _cacheName = cacheName;
            _config = new CacheConfigOverrides();
            _onResult = new List<Action<FunctionCacheGetResult<TK, TV>>>();
            _onFetch = new List<Action<FunctionCacheFetchResult<TK, TV>>>();
            _onError = new List<Action<FunctionCacheErrorEvent<TK>>>();
            _lock = new object();

            if (typeof(TK) == typeof(string))
                _keySerializer = k => k as string;
            else
                _keySerializer = k => k.ToString();
        }

        public FunctionCacheConfigurationManager<TK, TV> For(TimeSpan timeToLive)
        {
            _config.TimeToLive = timeToLive;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithKeySerializer(Func<TK, string> keySerializer)
        {
            _keySerializer = keySerializer;
            return this;
        } 

        public FunctionCacheConfigurationManager<TK, TV> WithMaxMemoryCacheMaxSizeMB(int maxSizeMB)
        {
            _config.MemoryCacheMaxSizeMB = maxSizeMB;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithMaxConcurrentFetches(int max)
        {
            _config.MaxConcurrentFetches = max;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithEarlyFetch(bool enabled = true)
        {
            _config.EarlyFetchEnabled = enabled;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> ContinueOnException(TV defaultValue = default(TV))
        {
            return ContinueOnException(() => defaultValue);
        }

        public FunctionCacheConfigurationManager<TK, TV> ContinueOnException(Func<TV> defaultValueFactory)
        {
            _defaultValueFactory = defaultValueFactory;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithCacheFactory(Func<ICache<TV>> cacheFactoryFunc)
        {
            _cacheFactoryFunc = cacheFactoryFunc;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnResult(Action<FunctionCacheGetResult<TK, TV>> onResult)
        {
            _onResult.Add(onResult);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnFetch(Action<FunctionCacheFetchResult<TK, TV>> onFetch)
        {
            _onFetch.Add(onFetch);
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> OnError(Action<FunctionCacheErrorEvent<TK>> onError)
        {
            _onError.Add(onError);
            return this;
        }

        public Func<TK, Task<TV>> Build()
        {
            lock (_lock)
            {
                if (_cachedFunc != null)
                    return _cachedFunc;

                var config = new CacheConfig(_config);

                var cacheFactoryFunc = _cacheFactoryFunc == null
                    ? () => MemoryCacheBuilder.Build<TV>(config.MemoryCacheMaxSizeMB)
                    : _cacheFactoryFunc;
                
                var cache = cacheFactoryFunc();

                var functionCache = new FunctionCache<TK, TV>(
                    _inputFunc,
                    _cacheName,
                    cache,
                    config.TimeToLive,
                    _keySerializer,
                    config.EarlyFetchEnabled,
                    _defaultValueFactory,
                    AggregateActions(_onResult),
                    AggregateActions(_onFetch),
                    AggregateActions(_onError));
                
                _cachedFunc = functionCache.Get;
                
                return _cachedFunc;
            }
        }

        private static Action<T> AggregateActions<T>(IList<Action<T>> actions)
        {
            if (actions == null || !actions.Any())
                return null;

            return actions.Aggregate((curr, next) => a => { curr(a); next(a); });
        }
        
        public static implicit operator Func<TK, Task<TV>>(FunctionCacheConfigurationManager<TK, TV> cache)
        {
            return cache.Build();
        }
    }
}