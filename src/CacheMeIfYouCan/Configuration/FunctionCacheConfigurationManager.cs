using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

[assembly: InternalsVisibleTo("ProxyFactoryAsm")]
[assembly: InternalsVisibleTo("CacheMeIfYouCan.Tests")]
namespace CacheMeIfYouCan.Configuration
{
    public class FunctionCacheConfigurationManager<TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFunc;
        private readonly Type _interfaceType;
        private readonly string _functionName;
        private readonly bool _multiKey = true;
        private TimeSpan? _timeToLive;
        private bool? _earlyFetchEnabled;
        private bool? _disableCache;
        private Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private Action<FunctionCacheErrorEvent<TK>> _onError;
        private Func<TK, string> _keySerializer;
        private Func<string, TK> _keyDeserializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private ILocalCacheFactory<TK, TV> _localCacheFactory;
        private ICacheFactory<TK, TV> _remoteCacheFactory;
        private Func<TV> _defaultValueFactory;
        private Func<Task<IList<TK>>> _keysToKeepAliveFunc;
        private Func<TK, Task<TV>> _cachedFunc;

        internal FunctionCacheConfigurationManager(
            Func<TK, Task<TV>> inputFunc,
            string functionName,
            CachedProxyConfig interfaceConfig = null)
            : this(ConvertFunc(inputFunc), functionName, interfaceConfig)
        {
            _multiKey = false;
        }

        internal FunctionCacheConfigurationManager(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName,
            CachedProxyConfig interfaceConfig = null)
        {
            _inputFunc = inputFunc;
            _functionName = functionName;

            if (interfaceConfig != null)
            {
                _interfaceType = interfaceConfig.InterfaceType;
                _keySerializer = interfaceConfig.KeySerializers.GetSerializer<TK>();
                _valueSerializer = interfaceConfig.ValueSerializers.GetSerializer<TV>();
                _valueDeserializer = interfaceConfig.ValueSerializers.GetDeserializer<TV>();
                _timeToLive = interfaceConfig.TimeToLive;
                _earlyFetchEnabled = interfaceConfig.EarlyFetchEnabled;
                _disableCache = interfaceConfig.DisableCache;

                if (interfaceConfig.LocalCacheFactory != null)
                    _localCacheFactory = new LocalCacheFactoryWrapper<TK, TV>(interfaceConfig.LocalCacheFactory);
                                
                if (interfaceConfig.RemoteCacheFactory != null)
                    _remoteCacheFactory = new CacheFactoryWrapper<TK, TV>(interfaceConfig.RemoteCacheFactory);

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
            else
            {
                _onResult = DefaultCacheConfig.Configuration.OnResult;
                _onFetch = DefaultCacheConfig.Configuration.OnFetch;
                _onError = DefaultCacheConfig.Configuration.OnError;
            }
        }

        public FunctionCacheConfigurationManager<TK, TV> For(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithKeySerializer(Func<TK, string> serializer, Func<string, TK> deserializer = null)
        {
            _keySerializer = serializer;
            _keyDeserializer = deserializer;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithKeySerializer(ISerializer serializer)
        {
            _keySerializer = serializer.Serialize;
            _keyDeserializer = serializer.Deserialize<TK>;
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithKeySerializer(ISerializer<TK> serializer)
        {
            _keySerializer = serializer.Serialize;
            _keyDeserializer = serializer.Deserialize;
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

        public FunctionCacheConfigurationManager<TK, TV> WithValueSerializer(ISerializer<TV> serializer)
        {
            _valueSerializer = serializer.Serialize;
            _valueDeserializer = serializer.Deserialize;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithEarlyFetch(bool enabled = true)
        {
            _earlyFetchEnabled = enabled;
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> DisableCache(bool disableCache = true)
        {
            _disableCache = disableCache;
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
        
        public FunctionCacheConfigurationManager<TK, TV> WithLocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            _localCacheFactory = new LocalCacheFactoryWrapper<TK, TV>(cacheFactory);
            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> WithLocalCacheFactory(Func<ILocalCache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _localCacheFactory = new LocalCacheFactoryFuncWrapper<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithLocalCache(ILocalCache<TK, TV> cache, bool requiresStringKeys = true)
        {
            _localCacheFactory = new LocalCacheFactoryFuncWrapper<TK, TV>(() => cache, requiresStringKeys);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithRemoteCacheFactory(ICacheFactory cacheFactory)
        {
            _remoteCacheFactory = new CacheFactoryWrapper<TK, TV>(cacheFactory);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithRemoteCacheFactory(Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _remoteCacheFactory = new CacheFactoryFuncWrapper<TK, TV>((c, _) => cacheFactoryFunc(c), requiresStringKeys);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithRemoteCacheFactory(Func<CacheFactoryConfig<TK, TV>, Action<Key<TK>>, ICache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _remoteCacheFactory = new CacheFactoryFuncWrapper<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> WithRemoteCache(ICache<TK, TV> cache, bool requiresStringKeys = true)
        {
            _remoteCacheFactory = new CacheFactoryFuncWrapper<TK, TV>((c, _) => cache, requiresStringKeys);
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnResult(Action<FunctionCacheGetResult<TK, TV>> onResult, bool append = false)
        {
            if (_onResult == null || !append)
                _onResult = onResult;
            else
                _onResult = x => { _onResult(x); onResult(x); };
            
            return this;
        }
        
        public FunctionCacheConfigurationManager<TK, TV> OnFetch(Action<FunctionCacheFetchResult<TK, TV>> onFetch, bool append = false)
        {
            if (_onFetch == null || !append)
                _onFetch = onFetch;
            else
                _onFetch = x => { _onFetch(x); onFetch(x); };

            return this;
        }

        public FunctionCacheConfigurationManager<TK, TV> OnError(Action<FunctionCacheErrorEvent<TK>> onError, bool append = false)
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
            var functionInfo = new FunctionInfo(_interfaceType, _functionName, typeof(TK), typeof(TV));
            var cacheConfig = new CacheFactoryConfig<TK, TV>
            {
                FunctionInfo = functionInfo,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = GetValueSerializer(),
                ValueDeserializer = GetValueDeserializer()
            };

            ICache<TK, TV> cache = null;
            IEqualityComparer<Key<TK>> keyComparer;
            if (_disableCache ?? DefaultCacheConfig.Configuration.DisableCache)
            {
                keyComparer = new NoMatchesComparer<Key<TK>>();
            }
            else
            {
                ILocalCacheFactory<TK, TV> localCacheFactory = null;
                if (_localCacheFactory != null)
                    localCacheFactory = _localCacheFactory;
                else if (DefaultCacheConfig.Configuration.LocalCacheFactory != null)
                    localCacheFactory = new LocalCacheFactoryWrapper<TK, TV>(DefaultCacheConfig.Configuration.LocalCacheFactory);

                ICacheFactory<TK, TV> remoteCacheFactory = null;
                if (_remoteCacheFactory != null)
                    remoteCacheFactory = _remoteCacheFactory;
                else if (DefaultCacheConfig.Configuration.RemoteCacheFactory != null)
                    remoteCacheFactory = new CacheFactoryWrapper<TK, TV>(DefaultCacheConfig.Configuration.RemoteCacheFactory);
                
                cache = CacheBuilder.Build(
                    localCacheFactory,
                    remoteCacheFactory,
                    cacheConfig,
                    out var requiresStringKeys);

                keyComparer = requiresStringKeys
                    ? (IEqualityComparer<Key<TK>>)new StringKeyComparer<TK>()
                    : new GenericKeyComparer<TK>();
            }

            var functionCache = new FunctionCache<TK, TV>(
                _inputFunc,
                functionInfo,
                cache,
                _timeToLive ?? DefaultCacheConfig.Configuration.TimeToLive,
                _keySerializer ?? GetKeySerializer(),
                _earlyFetchEnabled ?? DefaultCacheConfig.Configuration.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onError,
                keyComparer,
                _multiKey,
                _keysToKeepAliveFunc);
            
            _cachedFunc = functionCache.GetSingle;
            
            return _cachedFunc;
        }

        private Func<TK, string> GetKeySerializer()
        {
            var serializer = _keySerializer ?? DefaultCacheConfig.Configuration.KeySerializers.GetSerializer<TK>();
            
            if (serializer == null && !ProvidedSerializers.TryGetSerializer(out serializer))
                throw new Exception($"No key serializer defined for type '{typeof(TK).FullName}'");

            return serializer;
        }

        private Func<string, TK> GetKeyDeserializer()
        {
            var deserializer = _keyDeserializer ?? DefaultCacheConfig.Configuration.KeySerializers.GetDeserializer<TK>();

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer;
        }
        
        private Func<TV, string> GetValueSerializer()
        {
            var serializer = _valueSerializer ?? DefaultCacheConfig.Configuration.ValueSerializers.GetSerializer<TV>();
            
            if (serializer == null && !ProvidedSerializers.TryGetSerializer(out serializer))
                throw new Exception($"No serializer defined for type '{typeof(TV).FullName}'");

            return serializer;
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = _valueDeserializer ?? DefaultCacheConfig.Configuration.ValueSerializers.GetDeserializer<TV>();
            
            if (deserializer == null && !ProvidedSerializers.TryGetDeserializer(out deserializer))
                throw new Exception($"No serializer defined for type '{typeof(TV).FullName}'");

            return deserializer;
        }

        private static Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> ConvertFunc(Func<TK, Task<TV>> func)
        {
            return async keys =>
            {
                var key = keys.Single();

                var value = await func(key);
                
                return new Dictionary<TK, TV> { { key, value } };
            };
        }
        
        public static implicit operator Func<TK, Task<TV>>(FunctionCacheConfigurationManager<TK, TV> cacheConfig)
        {
            return cacheConfig.Build();
        }
    }
}