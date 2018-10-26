using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFunc;
        private readonly Type _interfaceType;
        private readonly string _functionName;
        private readonly bool _multiKey;
        private TimeSpan? _timeToLive;
        private bool? _earlyFetchEnabled;
        private bool? _disableCache;
        private Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private Action<FunctionCacheErrorEvent<TK>> _onError;
        private Action<CacheGetResult<TK, TV>> _onCacheGet;
        private Action<CacheSetResult<TK, TV>> _onCacheSet;
        private Func<TK, string> _keySerializer;
        private Func<string, TK> _keyDeserializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private ILocalCacheFactory<TK, TV> _localCacheFactory;
        private ICacheFactory<TK, TV> _remoteCacheFactory;
        private Func<TV> _defaultValueFactory;
        private Func<Task<IList<TK>>> _keysToKeepAliveFunc;

        internal FunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName,
            bool multiKey,
            CachedProxyConfig interfaceConfig = null)
        {
            _inputFunc = inputFunc;
            _functionName = functionName;
            _multiKey = multiKey;

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

                _onResult = interfaceConfig.OnResult;
                _onFetch = interfaceConfig.OnFetch;
                _onError = interfaceConfig.OnError;
                _onCacheGet = interfaceConfig.OnCacheGet;
                _onCacheSet = interfaceConfig.OnCacheSet;

                if (interfaceConfig.FunctionCacheConfigActions != null)
                {
                    var key = new MethodInfoKey(functionName, typeof(TK).FullName);

                    if (interfaceConfig.FunctionCacheConfigActions.TryGetValue(key, out var actionObj) &&
                        actionObj is Action<TConfig> action)
                    {
                        action((TConfig)this);
                    }
                }
            }
            else
            {
                _onResult = DefaultCacheConfig.Configuration.OnResult;
                _onFetch = DefaultCacheConfig.Configuration.OnFetch;
                _onError = DefaultCacheConfig.Configuration.OnError;
                _onCacheGet = DefaultCacheConfig.Configuration.OnCacheGet;
                _onCacheSet = DefaultCacheConfig.Configuration.OnCacheSet;
            }
        }

        public TConfig For(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            return (TConfig)this;
        }
        
        public TConfig WithKeySerializer(ISerializer serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize<TK>);
        }
        
        public TConfig WithKeySerializer(ISerializer<TK> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithKeySerializer(Func<TK, string> serializer, Func<string, TK> deserializer = null)
        {
            _keySerializer = serializer;
            _keyDeserializer = deserializer;
            return (TConfig)this;
        }
        
        public TConfig WithValueSerializer(ISerializer serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize<TV>);
        }
        
        public TConfig WithValueSerializer(ISerializer<TV> serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithValueSerializer(Func<TV, string> serializer, Func<string, TV> deserializer)
        {
            _valueSerializer = serializer;
            _valueDeserializer = deserializer;
            return (TConfig)this;
        }

        public TConfig EarlyFetchEnabled(bool enabled = true)
        {
            _earlyFetchEnabled = enabled;
            return (TConfig)this;
        }

        public TConfig DisableCache(bool disableCache = true)
        {
            _disableCache = disableCache;
            return (TConfig)this;
        }
        
        public TConfig ContinueOnException(TV defaultValue = default(TV))
        {
            return ContinueOnException(() => defaultValue);
        }

        public TConfig ContinueOnException(Func<TV> defaultValueFactory)
        {
            _defaultValueFactory = defaultValueFactory;
            return (TConfig)this;
        }
        
        public TConfig WithLocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            return WithLocalCacheFactory(cacheFactory.Build<TK, TV>, cacheFactory.RequiresStringKeys);
        }
        
        public TConfig WithLocalCache(ILocalCache<TK, TV> cache, bool requiresStringKeys = true)
        {
            return WithLocalCacheFactory(() => cache, requiresStringKeys);
        }
        
        public TConfig WithLocalCacheFactory(Func<ILocalCache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _localCacheFactory = new LocalCacheFactoryFuncWrapper<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            return (TConfig)this;
        }
        
        public TConfig WithRemoteCacheFactory(ICacheFactory cacheFactory)
        {
            return WithRemoteCacheFactory(cacheFactory.Build, cacheFactory.RequiresStringKeys);
        }
        
        public TConfig WithRemoteCacheFactory(Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            return WithRemoteCacheFactory((c, a) => cacheFactoryFunc(c), requiresStringKeys);
        }
        
        public TConfig WithRemoteCache(ICache<TK, TV> cache, bool requiresStringKeys = true)
        {
            return WithRemoteCacheFactory((c, a) => cache, requiresStringKeys);
        }
        
        public TConfig WithRemoteCacheFactory(Func<CacheFactoryConfig<TK, TV>, Action<Key<TK>>, ICache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _remoteCacheFactory = new CacheFactoryFuncWrapper<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            return (TConfig)this;
        }
        
        public TConfig OnResult(Action<FunctionCacheGetResult<TK, TV>> onResult, bool append = false)
        {
            if (_onResult == null || !append)
                _onResult = onResult;
            else
                _onResult = x => { _onResult(x); onResult(x); };
            
            return (TConfig)this;
        }
        
        public TConfig OnFetch(Action<FunctionCacheFetchResult<TK, TV>> onFetch, bool append = false)
        {
            if (_onFetch == null || !append)
                _onFetch = onFetch;
            else
                _onFetch = x => { _onFetch(x); onFetch(x); };

            return (TConfig)this;
        }

        public TConfig OnError(Action<FunctionCacheErrorEvent<TK>> onError, bool append = false)
        {
            if (_onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return (TConfig)this;
        }
        
        public TConfig OnCacheGet(Action<CacheGetResult<TK, TV>> onCacheGet, bool append = false)
        {
            if (onCacheGet == null || !append)
                _onCacheGet = onCacheGet;
            else
                _onCacheGet = x => { _onCacheGet(x); onCacheGet(x); };

            return (TConfig)this;
        }
        
        public TConfig OnCacheSet(Action<CacheSetResult<TK, TV>> onCacheSet, bool append = false)
        {
            if (onCacheSet == null || !append)
                _onCacheSet = onCacheSet;
            else
                _onCacheSet = x => { _onCacheSet(x); onCacheSet(x); };

            return (TConfig)this;
        }

        public TConfig WithKeysToKeepAlive(IList<TK> keysToKeepAlive)
        {
            return WithKeysToKeepAlive(() => keysToKeepAlive);
        }
        
        public TConfig WithKeysToKeepAlive(Func<IList<TK>> keysToKeepAliveFunc)
        {
            return WithKeysToKeepAlive(() => Task.FromResult(keysToKeepAliveFunc()));
        }
        
        // If you have lots of keys, make the function return them in priority order as keys are refreshed in that order.
        // Any that have not yet been processed when a new cycle is due will be ignored in order to ensure the highest
        // priority keys are always in the cache.
        public TConfig WithKeysToKeepAlive(Func<Task<IList<TK>>> keysToKeepAliveFunc)
        {
            _keysToKeepAliveFunc = keysToKeepAliveFunc;
            return (TConfig)this;
        }

        internal FunctionCache<TK, TV> BuildFunctionCache()
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
                    functionInfo,
                    localCacheFactory,
                    remoteCacheFactory,
                    cacheConfig,
                    _onCacheGet,
                    _onCacheSet,
                    out keyComparer);
            }

            return new FunctionCache<TK, TV>(
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

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer;
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = _valueDeserializer ?? DefaultCacheConfig.Configuration.ValueSerializers.GetDeserializer<TV>();
            
            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer;
        }
    }
}