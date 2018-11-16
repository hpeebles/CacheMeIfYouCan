﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFunc;
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
        private Action<CacheErrorEvent<TK>> _onCacheError;
        private Func<TK, string> _keySerializer;
        private Func<string, TK> _keyDeserializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private ILocalCacheFactory<TK, TV> _localCacheFactory;
        private ICacheFactory<TK, TV> _distributedCacheFactory;
        private string _keyspacePrefix;
        private Func<TV> _defaultValueFactory;
        private Func<Task<IList<TK>>> _keysToKeepAliveFunc;

        internal FunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName,
            bool multiKey,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
        {
            _inputFunc = inputFunc;
            _functionName = functionName;
            _multiKey = multiKey;

            if (interfaceConfig != null)
            {
                _keySerializer = interfaceConfig.KeySerializers.GetSerializer<TK>();
                _keyDeserializer = interfaceConfig.KeySerializers.GetDeserializer<TK>();
                _valueSerializer = interfaceConfig.ValueSerializers.GetSerializer<TV>();
                _valueDeserializer = interfaceConfig.ValueSerializers.GetDeserializer<TV>();
                _timeToLive = interfaceConfig.TimeToLive;
                _earlyFetchEnabled = interfaceConfig.EarlyFetchEnabled;
                _disableCache = interfaceConfig.DisableCache;

                if (interfaceConfig.LocalCacheFactory != null)
                    _localCacheFactory = new LocalCacheFactoryAdaptor<TK, TV>(interfaceConfig.LocalCacheFactory);
                                
                if (interfaceConfig.DistributedCacheFactory != null)
                    _distributedCacheFactory = new CacheFactoryGenericAdaptor<TK, TV>(interfaceConfig.DistributedCacheFactory);

                _keyspacePrefix = interfaceConfig.KeyspacePrefixFunc?.Invoke(proxyFunctionInfo);
                _onResult = interfaceConfig.OnResult;
                _onFetch = interfaceConfig.OnFetch;
                _onError = interfaceConfig.OnError;
                _onCacheGet = interfaceConfig.OnCacheGet;
                _onCacheSet = interfaceConfig.OnCacheSet;
                _onCacheError = interfaceConfig.OnCacheError;

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
                _onCacheError = DefaultCacheConfig.Configuration.OnCacheError;
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
        
        public TConfig WithLocalCache(ILocalCache<TK, TV> cache, bool requiresStringKeys = true)
        {
            return WithLocalCacheFactory(f => cache, requiresStringKeys);
        }
        
        public TConfig WithLocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            return WithLocalCacheFactory(cacheFactory.Build<TK, TV>, cacheFactory.RequiresStringKeys);
        }
        
        public TConfig WithLocalCacheFactory(Func<string, ILocalCache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            _localCacheFactory = new LocalCacheFactoryFuncAdaptor<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            return (TConfig)this;
        }
        
        public TConfig WithDistributedCache(ICache<TK, TV> cache, string keyspacePrefix)
        {
            return WithDistributedCacheFactory(c => cache, keyspacePrefix);
        } 
        
        public TConfig WithDistributedCache(ICache<TK, TV> cache, bool requiresStringKeys = true)
        {
            return WithDistributedCacheFactory(c => cache, requiresStringKeys);
        }
        
        public TConfig WithDistributedCacheFactory(ICacheFactory cacheFactory)
        {
            return WithDistributedCacheFactory(cacheFactory.Build);
        }
        
        public TConfig WithDistributedCacheFactory(ICacheFactory cacheFactory, string keyspacePrefix)
        {
            return WithDistributedCacheFactory(cacheFactory.Build, keyspacePrefix);
        }
        
        public TConfig WithDistributedCacheFactory(Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> cacheFactoryFunc, string keyspacePrefix)
        {
            SetDistributedCacheFactory(cacheFactoryFunc, true, keyspacePrefix);
            return (TConfig)this;
        }
        
        public TConfig WithDistributedCacheFactory(Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
        {
            SetDistributedCacheFactory(cacheFactoryFunc, requiresStringKeys, null);
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

        public TConfig OnCacheError(Action<CacheErrorEvent<TK>> onCacheError, bool append = false)
        {
            if (onCacheError == null || !append)
                _onCacheError = onCacheError;
            else
                _onCacheError = x => { _onCacheError(x); onCacheError(x); };

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
            var cacheConfig = new CacheFactoryConfig<TK, TV>
            {
                KeyspacePrefix = _keyspacePrefix,
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
                    localCacheFactory = new LocalCacheFactoryAdaptor<TK, TV>(DefaultCacheConfig.Configuration.LocalCacheFactory);

                ICacheFactory<TK, TV> distributedCacheFactory = null;
                if (_distributedCacheFactory != null)
                    distributedCacheFactory = _distributedCacheFactory;
                else if (DefaultCacheConfig.Configuration.DistributedCacheFactory != null)
                    distributedCacheFactory = new CacheFactoryGenericAdaptor<TK, TV>(DefaultCacheConfig.Configuration.DistributedCacheFactory);

                cache = CacheBuilder.Build(
                    _functionName,
                    localCacheFactory,
                    distributedCacheFactory,
                    cacheConfig,
                    _onCacheGet,
                    _onCacheSet,
                    _onCacheError,
                    out keyComparer);
            }

            return new FunctionCache<TK, TV>(
                _inputFunc,
                _functionName,
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

        private void SetDistributedCacheFactory(
            Func<CacheFactoryConfig<TK, TV>, ICache<TK, TV>> cacheFactoryFunc,
            bool requiresStringKeys,
            string keyspacePrefix)
        {
            _distributedCacheFactory = new CacheFactoryFuncAdaptor<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            _keyspacePrefix = keyspacePrefix;
        }

        private Func<TK, string> GetKeySerializer()
        {
            var serializer = _keySerializer ?? DefaultCacheConfig.Configuration.KeySerializers.GetSerializer<TK>();

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(TK).FullName}'"));
        }

        private Func<string, TK> GetKeyDeserializer()
        {
            var deserializer = _keyDeserializer ?? DefaultCacheConfig.Configuration.KeySerializers.GetDeserializer<TK>();

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(TK).FullName}'"));
        }
        
        private Func<TV, string> GetValueSerializer()
        {
            var serializer = _valueSerializer ?? DefaultCacheConfig.Configuration.ValueSerializers.GetSerializer<TV>();

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No value serializer defined for type '{typeof(TV).FullName}'"));
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = _valueDeserializer ?? DefaultCacheConfig.Configuration.ValueSerializers.GetDeserializer<TV>();
            
            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No value deserializer defined for type '{typeof(TV).FullName}'"));
        }
    }
}