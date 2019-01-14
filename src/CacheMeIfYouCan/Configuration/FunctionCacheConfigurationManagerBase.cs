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
        private Action<FunctionCacheException<TK>> _onException;
        private Action<CacheGetResult<TK, TV>> _onCacheGet;
        private Action<CacheSetResult<TK, TV>> _onCacheSet;
        private Action<CacheException<TK>> _onCacheException;
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

                if (interfaceConfig.LocalCacheFactory is NullLocalCacheFactory)
                    _localCacheFactory = new NullLocalCacheFactory<TK, TV>();
                else if (interfaceConfig.LocalCacheFactory != null)
                    _localCacheFactory = new LocalCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.LocalCacheFactory);
                
                if (interfaceConfig.DistributedCacheFactory is NullDistributedCacheFactory)
                    _distributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();
                else if (interfaceConfig.DistributedCacheFactory != null)
                    _distributedCacheFactory = new DistributedCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.DistributedCacheFactory);

                _keyspacePrefix = interfaceConfig.KeyspacePrefixFunc?.Invoke(proxyFunctionInfo);
                _onResult = interfaceConfig.OnResult;
                _onFetch = interfaceConfig.OnFetch;
                _onException = interfaceConfig.OnException;
                _onCacheGet = interfaceConfig.OnCacheGet;
                _onCacheSet = interfaceConfig.OnCacheSet;
                _onCacheException = interfaceConfig.OnCacheException;

                if (interfaceConfig.FunctionCacheConfigActions != null)
                {
                    var key = new MethodInfoKey(interfaceConfig.InterfaceType, proxyFunctionInfo.MethodInfo);

                    if (interfaceConfig.FunctionCacheConfigActions.TryGetValue(key, out var actionObj))
                    {
                        var action = (Action<TConfig>)actionObj;
                        
                        action((TConfig)this);
                    }
                }
            }
            else
            {
                _onResult = DefaultSettings.Cache.OnResult;
                _onFetch = DefaultSettings.Cache.OnFetch;
                _onException = DefaultSettings.Cache.OnException;
                _onCacheGet = DefaultSettings.Cache.OnCacheGet;
                _onCacheSet = DefaultSettings.Cache.OnCacheSet;
                _onCacheException = DefaultSettings.Cache.OnCacheException;
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
        
        public TConfig ContinueOnException(TV defaultValue = default)
        {
            return ContinueOnException(() => defaultValue);
        }

        public TConfig ContinueOnException(Func<TV> defaultValueFactory)
        {
            _defaultValueFactory = defaultValueFactory;
            return (TConfig)this;
        }
        
        public TConfig WithLocalCache(ILocalCache<TK, TV> cache)
        {
            return WithLocalCacheFactory(f => cache);
        }
        
        public TConfig WithLocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            return WithLocalCacheFactory(cacheFactory.Build<TK, TV>);
        }
        
        public TConfig WithLocalCacheFactory(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            return WithLocalCacheFactory(cacheFactory.Build);
        }
        
        public TConfig WithLocalCacheFactory(Func<string, ILocalCache<TK, TV>> cacheFactoryFunc)
        {
            _localCacheFactory = new LocalCacheFactoryFromFuncAdapter<TK, TV>(cacheFactoryFunc);
            return (TConfig)this;
        }
        
        public TConfig SkipLocalCache(bool skipLocalCache = true)
        {
            if (skipLocalCache)
                _localCacheFactory = new NullLocalCacheFactory<TK, TV>();
            
            return (TConfig)this;
        }
        
        public TConfig WithDistributedCache(
            IDistributedCache<TK, TV> cache,
            string keyspacePrefix = null)
        {
            return WithDistributedCacheFactory(c => cache, keyspacePrefix);
        }
        
        public TConfig WithDistributedCacheFactory(
            IDistributedCacheFactory cacheFactory,
            string keyspacePrefix = null)
        {
            return WithDistributedCacheFactory(cacheFactory.Build, keyspacePrefix);
        }

        public TConfig WithDistributedCacheFactory(
            IDistributedCacheFactory<TK, TV> cacheFactory,
            string keyspacePrefix = null)
        {
            return WithDistributedCacheFactory(cacheFactory.Build, keyspacePrefix);
        }
        
        public TConfig WithDistributedCacheFactory(
            Func<DistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> cacheFactoryFunc,
            string keyspacePrefix = null)
        {
            _distributedCacheFactory = new DistributedCacheFactoryFromFuncAdapter<TK, TV>(cacheFactoryFunc);
            _keyspacePrefix = keyspacePrefix;
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCache(bool skipDistributedCache = true)
        {
            if (skipDistributedCache)
            {
                _distributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();
                _keyspacePrefix = null;
            }

            return (TConfig)this;
        }
        
        public TConfig WithCacheFactoryPreset(int id)
        {
            return WithCacheFactoryPresetImpl(CacheFactoryPresetKeyFactory.Create(id));
        }
        
        public TConfig WithCacheFactoryPreset<TEnum>(TEnum id) where TEnum : struct, Enum
        {
            return WithCacheFactoryPresetImpl(CacheFactoryPresetKeyFactory.Create(id));
        }
        
        private TConfig WithCacheFactoryPresetImpl(CacheFactoryPresetKey key)
        {
            if (!DefaultSettings.Cache.CacheFactoryPresets.TryGetValue(key, out var cacheFactories))
                throw new Exception("No cache factory preset found. " + key);

            if (cacheFactories.local == null)
                SkipLocalCache();
            else
                WithLocalCacheFactory(cacheFactories.local);

            if (cacheFactories.distributed == null)
                SkipDistributedCache();
            else
                WithDistributedCacheFactory(cacheFactories.distributed);

            return (TConfig)this;
        }
        
        public TConfig OnResult(
            Action<FunctionCacheGetResult<TK, TV>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onResult = ActionsHelper.Combine(_onResult, onResult, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnFetch(
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onFetch = ActionsHelper.Combine(_onFetch, onFetch, behaviour);
            return (TConfig)this;
        }

        public TConfig OnException(
            Action<FunctionCacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onException = ActionsHelper.Combine(_onException, onException, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnCacheGet(
            Action<CacheGetResult<TK, TV>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheGet = ActionsHelper.Combine(_onCacheGet, onCacheGet, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnCacheSet(
            Action<CacheSetResult<TK, TV>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheSet = ActionsHelper.Combine(_onCacheSet, onCacheSet, behaviour);
            return (TConfig)this;
        }

        public TConfig OnCacheException(
            Action<CacheException<TK>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onCacheException = ActionsHelper.Combine(_onCacheException, onCacheException, behaviour);
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
                var timeToLive = _timeToLive ?? DefaultSettings.Cache.TimeToLive;
                timeToLiveFactory = (k, v) => timeToLive;
            }

            return new FunctionCacheSingle<TK, TV>(
                _inputFuncSingle,
                _functionName,
                cache,
                timeToLiveFactory,
                _keySerializer ?? GetKeySerializer(),
                _earlyFetchEnabled ?? DefaultSettings.Cache.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onException,
                keyComparer);
        }
        
        internal FunctionCacheMulti<TK, TV> BuildFunctionCacheMulti(Func<IDictionary<TK, TV>> dictionaryFactoryFunc)
        {
            if (!_multiKey)
                throw new Exception("You can't build a FunctionCacheMulti since your function is single key");

            var cache = BuildCache(out var keyComparer);
            
            return new FunctionCacheMulti<TK, TV>(
                _inputFuncMulti,
                _functionName,
                cache,
                _timeToLive ?? DefaultSettings.Cache.TimeToLive,
                _keySerializer ?? GetKeySerializer(),
                _earlyFetchEnabled ?? DefaultSettings.Cache.EarlyFetchEnabled,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onException,
                keyComparer,
                dictionaryFactoryFunc);
        }

        private ICacheInternal<TK, TV> BuildCache(out IEqualityComparer<Key<TK>> keyComparer)
        {
            var cacheConfig = new DistributedCacheConfig<TK, TV>(_functionName)
            {
                KeyspacePrefix = _keyspacePrefix,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = GetValueSerializer(),
                ValueDeserializer = GetValueDeserializer()
            };

            ICacheInternal<TK, TV> cache = null;
            if (_disableCache ?? DefaultSettings.Cache.DisableCache)
            {
                keyComparer = new NoMatchesComparer<Key<TK>>();
            }
            else
            {
                ILocalCacheFactory<TK, TV> localCacheFactory = null;
                if (_localCacheFactory != null)
                    localCacheFactory = _localCacheFactory;
                else if (DefaultSettings.Cache.LocalCacheFactory != null)
                    localCacheFactory = new LocalCacheFactoryToGenericAdapter<TK, TV>(DefaultSettings.Cache.LocalCacheFactory);

                IDistributedCacheFactory<TK, TV> distributedCacheFactory = null;
                if (_distributedCacheFactory != null)
                    distributedCacheFactory = _distributedCacheFactory;
                else if (DefaultSettings.Cache.DistributedCacheFactory != null)
                    distributedCacheFactory = new DistributedCacheFactoryToGenericAdapter<TK, TV>(DefaultSettings.Cache.DistributedCacheFactory);

                keyComparer = KeyComparerResolver.Get<TK>();

                cache = CacheBuilder.Build(
                    _functionName,
                    localCacheFactory,
                    distributedCacheFactory,
                    cacheConfig,
                    _onCacheGet,
                    _onCacheSet,
                    _onCacheException,
                    keyComparer);
            }

            return cache;
        }

        private Func<TK, string> GetKeySerializer()
        {
            var serializer = _keySerializer;
            
            if (serializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetSerializer<TK>(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(TK).FullName}'"));
        }

        private Func<string, TK> GetKeyDeserializer()
        {
            var deserializer = _keyDeserializer;
            
            if (deserializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetDeserializer<TK>(out deserializer);

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(TK).FullName}'"));
        }
        
        private Func<TV, string> GetValueSerializer()
        {
            var serializer = _valueSerializer;

            if (serializer == null)
                DefaultSettings.Cache.ValueSerializers.TryGetSerializer<TV>(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No value serializer defined for type '{typeof(TV).FullName}'"));
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = _valueDeserializer;
            
            if (deserializer == null)
                DefaultSettings.Cache.ValueSerializers.TryGetDeserializer<TV>(out deserializer);
            
            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No value deserializer defined for type '{typeof(TV).FullName}'"));
        }
    }
}