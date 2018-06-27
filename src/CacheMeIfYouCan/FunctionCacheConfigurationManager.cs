using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;

[assembly: InternalsVisibleTo("ProxyFactoryAsm")]
[assembly: InternalsVisibleTo("CacheMeIfYouCan.Tests")]
namespace CacheMeIfYouCan
{
    public class FunctionCacheConfigurationManager<TK, TV>
    {
        private readonly Func<TK, Task<TV>> _inputFunc;
        private readonly string _interfaceName;
        private readonly string _functionName;
        private TimeSpan? _timeToLive;
        private int? _memoryCacheMaxSizeMB;
        private bool? _earlyFetchEnabled;
        private Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private Action<FunctionCacheErrorEvent<TK>> _onError;
        private Func<TK, string> _keySerializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private Func<MemoryCache<TV>, Func<TV, string>, Func<string, TV>, ICache<TV>> _cacheFactoryFunc;
        private Func<TV> _defaultValueFactory;
        private Func<Task<IList<TK>>> _keysToKeepAliveFunc;
        private Func<TK, Task<TV>> _cachedFunc;
        
        internal FunctionCacheConfigurationManager(Func<TK, Task<TV>> inputFunc, string functionName, CachedProxyConfig interfaceConfig = null)
        {
            _inputFunc = inputFunc;
            _functionName = functionName;

            if (interfaceConfig != null)
            {
                _interfaceName = interfaceConfig.InterfaceName;
                _keySerializer = interfaceConfig.Serializers.GetSerializer<TK>();
                _valueSerializer = interfaceConfig.Serializers.GetSerializer<TV>();
                _valueDeserializer = interfaceConfig.Serializers.GetDeserializer<TV>();
                _timeToLive = interfaceConfig.TimeToLive;
                _memoryCacheMaxSizeMB = interfaceConfig.MemoryCacheMaxSizeMB;
                _earlyFetchEnabled = interfaceConfig.EarlyFetchEnabled;

                if (interfaceConfig.CacheFactory != null)
                {
                    _cacheFactoryFunc = (memoryCache, serializer, deserializer) => interfaceConfig.CacheFactory.Build(memoryCache, serializer, deserializer);
                }
                
                if (interfaceConfig.OnResult != null)
                    _onResult = interfaceConfig.OnResult;
                
                if (interfaceConfig.OnFetch != null)
                    _onFetch = interfaceConfig.OnFetch;
                
                if (interfaceConfig.OnError != null)
                    _onError = interfaceConfig.OnError;

                if (interfaceConfig.FunctionCacheConfigActions != null)
                {
                    var key = new MethodInfoKey(functionName, typeof(TK).FullName);

                    if (interfaceConfig.FunctionCacheConfigActions.TryGetValue(key, out var actionObj) &&
                        actionObj is Action<FunctionCacheConfigurationManager<TK, TV>> action)
                    {
                            action(this);
                    }
                }
            }
        }

        public FunctionCacheConfigurationManager<TK, TV> For(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithKeySerializer(Func<TK, string> serializer)
        {
            _keySerializer = serializer;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithValueSerializer(Func<TV, string> serializer, Func<string, TV> deserializer)
        {
            _valueSerializer = serializer;
            _valueDeserializer = deserializer;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithValueSerializer(ISerializer serializer)
        {
            _valueSerializer = serializer.Serialize;
            _valueDeserializer = serializer.Deserialize<TV>;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithMaxMemoryCacheMaxSizeMB(int maxSizeMB)
        {
            _memoryCacheMaxSizeMB = maxSizeMB;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithEarlyFetch(bool enabled = true)
        {
            _earlyFetchEnabled = enabled;
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
            _cacheFactoryFunc = (m, s, d) => cacheFactoryFunc();
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithCacheFactory(ICacheFactory cacheFactory)
        {
            _cacheFactoryFunc = cacheFactory.Build;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnResult(Action<FunctionCacheGetResult<TK, TV>> onResult, bool append = true)
        {
            if (_onResult == null || !append)
                _onResult = onResult;
            else
                _onResult = x => { _onResult(x); onResult(x); };
            
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnFetch(Action<FunctionCacheFetchResult<TK, TV>> onFetch, bool append = true)
        {
            if (_onFetch == null || !append)
                _onFetch = onFetch;
            else
                _onFetch = x => { _onFetch(x); onFetch(x); };

            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> OnError(Action<FunctionCacheErrorEvent<TK>> onError, bool append = true)
        {
            if (_onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithKeysToKeepAlive(IList<TK> keysToKeepAlive)
        {
            return WithKeysToKeepAlive(() => keysToKeepAlive);
        }

        public FunctionCacheConfigurationManager<TK, TV> WithKeysToKeepAlive(Func<IList<TK>> keysToKeepAliveFunc)
        {
            return WithKeysToKeepAlive(() => Task.FromResult(keysToKeepAliveFunc()));
        }
        
        // If you have lots of keys, make the function return them in priority order as keys are refreshed in that order.
        // Any that have not yet been processed when a new cycle is due will be ignored in order to ensure the highest
        // priority keys are always in the cache.
        public FunctionCacheConfigurationManager<TK, TV> WithKeysToKeepAlive(Func<Task<IList<TK>>> keysToKeepAliveFunc)
        {
            _keysToKeepAliveFunc = keysToKeepAliveFunc;
            return this;
        } 

        public Func<TK, Task<TV>> Build()
        {
            var memoryCache = MemoryCacheBuilder.Build<TV>(_memoryCacheMaxSizeMB ?? DefaultCacheSettings.MemoryCacheMaxSizeMB);

            ICache<TV> cache;
            if (_cacheFactoryFunc == null)
            {
                cache = memoryCache;
            }
            else
            {
                var valueSerializer = _valueSerializer ?? DefaultCacheSettings.Serializers.GetSerializer<TV>();
                var valueDeserializer = _valueDeserializer ?? DefaultCacheSettings.Serializers.GetDeserializer<TV>();

                cache = _cacheFactoryFunc(memoryCache, valueSerializer, valueDeserializer);
            }

            var functionCache = new FunctionCache<TK, TV>(
                _inputFunc,
                _interfaceName,
                _functionName,
                cache,
                _timeToLive ?? DefaultCacheSettings.TimeToLive,
                _keySerializer ?? DefaultCacheSettings.Serializers.GetSerializer<TK>() ?? ToStringSerializer,
                _earlyFetchEnabled ?? DefaultCacheSettings.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onError,
                _keysToKeepAliveFunc);
            
            _cachedFunc = functionCache.Get;
            
            return _cachedFunc;
        }

        private static string ToStringSerializer(TK key)
        {
            return key.ToString();
        }
        
        public static implicit operator Func<TK, Task<TV>>(FunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}