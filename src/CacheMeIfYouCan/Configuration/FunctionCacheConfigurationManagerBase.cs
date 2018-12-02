using System;
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
        private readonly Func<TK, Task<TV>> _inputFuncSingle;
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _inputFuncMulti;
        private readonly string _functionName;
        private readonly bool _multiKey;
        private TimeSpan? _timeToLive;
        private Func<TK, TV, TimeSpan> _timeToLiveFactory;
        private bool? _earlyFetchEnabled;
        private bool? _disableCache;
        private Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private Action<FunctionCacheException<TK>> _onError;
        private Action<CacheGetResult<TK, TV>> _onCacheGet;
        private Action<CacheSetResult<TK, TV>> _onCacheSet;
        private Action<CacheException<TK>> _onCacheError;
        private Func<TK, string> _keySerializer;
        private Func<string, TK> _keyDeserializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private ILocalCacheFactory<TK, TV> _localCacheFactory;
        private IDistributedCacheFactory<TK, TV> _distributedCacheFactory;
        private string _keyspacePrefix;
        private Func<TV> _defaultValueFactory;

        internal FunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFuncSingle,
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
            : this(functionName, interfaceConfig, proxyFunctionInfo)
        {
            _inputFuncSingle = inputFuncSingle;
            _multiKey = false;
        }
        
        internal FunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFuncMulti,
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
            : this(functionName, interfaceConfig, proxyFunctionInfo)
        {
            _inputFuncMulti = inputFuncMulti;
            _multiKey = true;
        }
        
        private FunctionCacheConfigurationManagerBase(
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
        {
            _functionName = functionName;

            if (interfaceConfig != null)
            {
                if (interfaceConfig.KeySerializers.TryGetSerializer<TK>(out var keySerializer))
                    _keySerializer = keySerializer;

                if (interfaceConfig.KeySerializers.TryGetDeserializer<TK>(out var keyDeserializer))
                    _keyDeserializer = keyDeserializer;

                if (interfaceConfig.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer))
                    _valueSerializer = valueSerializer;

                if (interfaceConfig.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer))
                    _valueDeserializer = valueDeserializer;

                _timeToLive = interfaceConfig.TimeToLive;
                _earlyFetchEnabled = interfaceConfig.EarlyFetchEnabled;
                _disableCache = interfaceConfig.DisableCache;

                if (interfaceConfig.LocalCacheFactory != null)
                    _localCacheFactory = new LocalCacheFactoryAdaptor<TK, TV>(interfaceConfig.LocalCacheFactory);
                                
                if (interfaceConfig.DistributedCacheFactory != null)
                    _distributedCacheFactory = new DistributedCacheFactoryGenericAdaptor<TK, TV>(interfaceConfig.DistributedCacheFactory);

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

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            _timeToLiveFactory = (k, v) => timeToLive;
            return (TConfig)this;
        }
        
        protected TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            _timeToLiveFactory = timeToLiveFactory;
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
        
        public TConfig WithDistributedCache(IDistributedCache<TK, TV> cache, string keyspacePrefix)
        {
            return WithDistributedCacheFactory(c => cache, keyspacePrefix);
        } 
        
        public TConfig WithDistributedCache(IDistributedCache<TK, TV> cache, bool requiresStringKeys = true)
        {
            return WithDistributedCacheFactory(c => cache, requiresStringKeys);
        }
        
        public TConfig WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory)
        {
            return WithDistributedCacheFactory(cacheFactory.Build);
        }
        
        public TConfig WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory, string keyspacePrefix)
        {
            return WithDistributedCacheFactory(cacheFactory.Build, keyspacePrefix);
        }
        
        public TConfig WithDistributedCacheFactory(Func<DistributedCacheFactoryConfig<TK, TV>, IDistributedCache<TK, TV>> cacheFactoryFunc, string keyspacePrefix)
        {
            SetDistributedCacheFactory(cacheFactoryFunc, true, keyspacePrefix);
            return (TConfig)this;
        }
        
        public TConfig WithDistributedCacheFactory(Func<DistributedCacheFactoryConfig<TK, TV>, IDistributedCache<TK, TV>> cacheFactoryFunc, bool requiresStringKeys = true)
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

        public TConfig OnError(Action<FunctionCacheException<TK>> onError, bool append = false)
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

        public TConfig OnCacheError(Action<CacheException<TK>> onCacheError, bool append = false)
        {
            if (onCacheError == null || !append)
                _onCacheError = onCacheError;
            else
                _onCacheError = x => { _onCacheError(x); onCacheError(x); };

            return (TConfig)this;
        }
        
        internal FunctionCacheSingle<TK, TV> BuildFunctionCacheSingle()
        {
            if (_multiKey)
                throw new Exception("You can't build a FunctionCacheSingle since your function is multi key");

            var cache = BuildCache(out var keyComparer);

            Func<TK, TV, TimeSpan> timeToLiveFactory;
            if (_timeToLiveFactory != null)
            {
                timeToLiveFactory = _timeToLiveFactory;
            }
            else
            {
                var timeToLive = _timeToLive ?? DefaultCacheConfig.Configuration.TimeToLive;
                timeToLiveFactory = (k, v) => timeToLive;
            }

            return new FunctionCacheSingle<TK, TV>(
                _inputFuncSingle,
                _functionName,
                cache,
                timeToLiveFactory,
                _keySerializer ?? GetKeySerializer(),
                _earlyFetchEnabled ?? DefaultCacheConfig.Configuration.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onError,
                keyComparer);
        }
        
        internal FunctionCacheMulti<TK, TV> BuildFunctionCacheMulti()
        {
            if (!_multiKey)
                throw new Exception("You can't build a FunctionCacheMulti since your function is single key");

            var cache = BuildCache(out var keyComparer);
            
            return new FunctionCacheMulti<TK, TV>(
                _inputFuncMulti,
                _functionName,
                cache,
                _timeToLive ?? DefaultCacheConfig.Configuration.TimeToLive,
                _keySerializer ?? GetKeySerializer(),
                _earlyFetchEnabled ?? DefaultCacheConfig.Configuration.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onError,
                keyComparer);
        }

        private ICache<TK, TV> BuildCache(out IEqualityComparer<Key<TK>> keyComparer)
        {
            var cacheConfig = new DistributedCacheFactoryConfig<TK, TV>
            {
                KeyspacePrefix = _keyspacePrefix,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = GetValueSerializer(),
                ValueDeserializer = GetValueDeserializer()
            };

            ICache<TK, TV> cache = null;
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

                IDistributedCacheFactory<TK, TV> distributedCacheFactory = null;
                if (_distributedCacheFactory != null)
                    distributedCacheFactory = _distributedCacheFactory;
                else if (DefaultCacheConfig.Configuration.DistributedCacheFactory != null)
                    distributedCacheFactory = new DistributedCacheFactoryGenericAdaptor<TK, TV>(DefaultCacheConfig.Configuration.DistributedCacheFactory);

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

            return cache;
        }

        private void SetDistributedCacheFactory(
            Func<DistributedCacheFactoryConfig<TK, TV>, IDistributedCache<TK, TV>> cacheFactoryFunc,
            bool requiresStringKeys,
            string keyspacePrefix)
        {
            _distributedCacheFactory = new DistributedCacheFactoryFuncAdaptor<TK, TV>(cacheFactoryFunc, requiresStringKeys);
            _keyspacePrefix = keyspacePrefix;
        }

        private Func<TK, string> GetKeySerializer()
        {
            var serializer = _keySerializer;
            
            if (serializer == null)
                DefaultCacheConfig.Configuration.KeySerializers.TryGetSerializer<TK>(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(TK).FullName}'"));
        }

        private Func<string, TK> GetKeyDeserializer()
        {
            var deserializer = _keyDeserializer;
            
            if (deserializer == null)
                DefaultCacheConfig.Configuration.KeySerializers.TryGetDeserializer<TK>(out deserializer);

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(TK).FullName}'"));
        }
        
        private Func<TV, string> GetValueSerializer()
        {
            var serializer = _valueSerializer;

            if (serializer == null)
                DefaultCacheConfig.Configuration.ValueSerializers.TryGetSerializer<TV>(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No value serializer defined for type '{typeof(TV).FullName}'"));
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = _valueDeserializer;
            
            if (deserializer == null)
                DefaultCacheConfig.Configuration.ValueSerializers.TryGetDeserializer<TV>(out deserializer);
            
            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No value deserializer defined for type '{typeof(TV).FullName}'"));
        }
    }
}