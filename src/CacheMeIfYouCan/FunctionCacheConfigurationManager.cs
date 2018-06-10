﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan
{
    public class FunctionCacheConfigurationManager<T>
    {
        private readonly Func<string, Task<T>> _inputFunc;
        private readonly CacheConfigOverrides _config;
        private readonly object _lock;
        private Func<ICache<T>> _cacheFactoryFunc;
        private Func<T> _defaultValueFactory;
        private IList<Action<FunctionCacheGetResult<T>>> _onResult;
        private IList<Action<FunctionCacheFetchResult<T>>> _onFetch;
        private Func<string, Task<T>> _cachedFunc;
        
        internal FunctionCacheConfigurationManager(Func<string, Task<T>> inputFunc)
        {
            _inputFunc = inputFunc;
            _config = new CacheConfigOverrides();
            _lock = new object();
            _onResult = new List<Action<FunctionCacheGetResult<T>>>();
            _onFetch = new List<Action<FunctionCacheFetchResult<T>>>();
        }

        public FunctionCacheConfigurationManager<T> For(TimeSpan timeToLive)
        {
            _config.TimeToLive = timeToLive;
            return this;
        }

        public FunctionCacheConfigurationManager<T> WithMaxItemsInMemoryCache(long maxItems)
        {
            _config.MaxItemsInMemoryCache = maxItems;
            return this;
        }

        public FunctionCacheConfigurationManager<T> WithMaxConcurrentFetches(int max)
        {
            _config.MaxConcurrentFetches = max;
            return this;
        }

        public FunctionCacheConfigurationManager<T> WithEarlyFetch(bool enabled = true)
        {
            _config.EarlyFetchEnabled = enabled;
            return this;
        }

        public FunctionCacheConfigurationManager<T> WithLogger(ILogger logger)
        {
            _config.Logger = logger;
            return this;
        }

        public FunctionCacheConfigurationManager<T> ContinueOnException(T defaultValue = default(T))
        {
            return ContinueOnException(() => defaultValue);
        }

        public FunctionCacheConfigurationManager<T> ContinueOnException(Func<T> defaultValueFactory)
        {
            _defaultValueFactory = defaultValueFactory;
            return this;
        }

        public FunctionCacheConfigurationManager<T> WithCacheFactory(Func<ICache<T>> cacheFactoryFunc)
        {
            _cacheFactoryFunc = cacheFactoryFunc;
            return this;
        }
        
        public FunctionCacheConfigurationManager<T> OnResult(Action<FunctionCacheGetResult<T>> onResult)
        {
            _onResult.Add(onResult);
            return this;
        }
        
        public FunctionCacheConfigurationManager<T> OnFetch(Action<FunctionCacheFetchResult<T>> onFetch)
        {
            _onFetch.Add(onFetch);
            return this;
        }

        public virtual Func<string, Task<T>> Build()
        {
            lock (_lock)
            {
                if (_cachedFunc != null)
                    return _cachedFunc;

                var config = new CacheConfig(_config);

                var cacheFactoryFunc = _cacheFactoryFunc == null
                    ? () => MemoryCacheBuilder.Build<T>(config.MaxItemsInMemoryCache)
                    : _cacheFactoryFunc;
                
                var cache = cacheFactoryFunc();

                var onResult = _onResult.Any()
                    ? _onResult.Aggregate((curr, next) => r => { curr(r); next(r); })
                    : null;
                
                var onFetch = _onFetch.Any()
                    ? _onFetch.Aggregate((curr, next) => f => { curr(f); next(f); })
                    : null;
                
                var functionCache = new FunctionCache<T>(
                    _inputFunc,
                    cache,
                    config.TimeToLive,
                    config.EarlyFetchEnabled,
                    config.Logger,
                    _defaultValueFactory,
                    onResult,
                    onFetch);
                
                _cachedFunc = functionCache.Get;
                
                return _cachedFunc;
            }
        }

        public static implicit operator Func<string, Task<T>>(FunctionCacheConfigurationManager<T> cache)
        {
            return cache.Build();
        }
    }
}