using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        private readonly Func<TK, Task<TV>> _singleKeyInputFunc;
        private readonly Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> _enumerableKeyInputFunc;
        private readonly string _functionName;
        private readonly bool _isEnumerableKey;
        private readonly KeySerializers _keySerializers;
        private readonly EqualityComparers _keyComparers;
        private TimeSpan? _timeToLive;
        private Func<TK, TV, TimeSpan> _timeToLiveFactory;
        private bool? _disableCache;
        private Action<FunctionCacheGetResult<TK, TV>> _onResult;
        private Action<FunctionCacheFetchResult<TK, TV>> _onFetch;
        private Action<FunctionCacheException<TK>> _onException;
        private Action<CacheGetResult<TK, TV>> _onCacheGet;
        private Action<CacheSetResult<TK, TV>> _onCacheSet;
        private Action<CacheException<TK>> _onCacheException;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private ILocalCacheFactory<TK, TV> _localCacheFactory;
        private IDistributedCacheFactory<TK, TV> _distributedCacheFactory;
        private string _keyspacePrefix;
        private Func<TV> _defaultValueFactory;
        private IObservable<TK> _keysToRemoveObservable;

        internal FunctionCacheConfigurationManagerBase(
            Func<TK, Task<TV>> inputFunc,
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
            : this(functionName, interfaceConfig, proxyFunctionInfo)
        {
            _singleKeyInputFunc = inputFunc;
            _isEnumerableKey = false;
        }
        
        internal FunctionCacheConfigurationManagerBase(
            Func<IEnumerable<TK>, Task<IDictionary<TK, TV>>> inputFunc,
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
            : this(functionName, interfaceConfig, proxyFunctionInfo)
        {
            _enumerableKeyInputFunc = inputFunc;
            _isEnumerableKey = true;
        }
        
        private FunctionCacheConfigurationManagerBase(
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
        {
            _functionName = functionName;

            if (interfaceConfig != null)
            {
                _keySerializers = interfaceConfig.KeySerializers.Clone();
                _keyComparers = interfaceConfig.KeyComparers.Clone();

                if (interfaceConfig.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer))
                    _valueSerializer = valueSerializer;

                if (interfaceConfig.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer))
                    _valueDeserializer = valueDeserializer;

                _timeToLive = interfaceConfig.TimeToLive;
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
                _keySerializers = new KeySerializers();
                _keyComparers = new EqualityComparers();
                _onResult = DefaultSettings.Cache.OnResultAction;
                _onFetch = DefaultSettings.Cache.OnFetchAction;
                _onException = DefaultSettings.Cache.OnExceptionAction;
                _onCacheGet = DefaultSettings.Cache.OnCacheGetAction;
                _onCacheSet = DefaultSettings.Cache.OnCacheSetAction;
                _onCacheException = DefaultSettings.Cache.OnCacheExceptionAction;
            }
        }

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            _timeToLive = timeToLive;
            _timeToLiveFactory = null;
            return (TConfig)this;
        }

        private protected TConfig WithTimeToLiveFactory(Func<TK, TV, TimeSpan> timeToLiveFactory)
        {
            _timeToLive = null;
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
            return WithKeySerializerInternal<TK>(serializer, deserializer);
        }

        private protected TConfig WithKeySerializerInternal<T>(ISerializer<T> serializer)
        {
            return WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize);
        }

        private protected TConfig WithKeySerializerInternal<T>(Func<T, string> serializer, Func<string, T> deserializer = null)
        {
            _keySerializers.Set(serializer, deserializer);
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

        private protected TConfig WithKeyComparer<T>(IEqualityComparer<T> comparer)
        {
            _keyComparers.Set(comparer);
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
        
        public TConfig WithDistributedCache(IDistributedCache<TK, TV> cache)
        {
            return WithDistributedCacheFactory(c => cache);
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
            {
                var existingKeys = String.Join(
                    ", ",
                    DefaultSettings.Cache.CacheFactoryPresets.Keys.Select(k => k.ToString()));
                
                throw new Exception($"Cache factory preset not found. Requested Key: {key}. Existing Keys: {existingKeys}");
            }

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

        public TConfig WithKeysToRemoveObservable(IObservable<TK> keysToRemoveObservable)
        {
            _keysToRemoveObservable = keysToRemoveObservable;
            return (TConfig)this;
        }
        
        private protected SingleKeyFunctionCache<TK, TV> BuildFunctionCacheSingle()
        {
            if (_isEnumerableKey)
                throw new Exception($"You can't build a {nameof(SingleKeyFunctionCache<TK, TV>)} since your function has an enumerable key");

            var keyComparer = GetKeyComparer(_keyComparers);
            
            var cache = BuildCache(keyComparer);

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

            var keySerializer = GetKeySerializer();

            if (_keysToRemoveObservable != null)
            {
                _keysToRemoveObservable
                    .SelectMany(k => Observable.FromAsync(() => cache.Remove(new Key<TK>(k, keySerializer)).AsTask()))
                    .Retry()
                    .Subscribe();
            }
            
            var functionCache = new SingleKeyFunctionCache<TK, TV>(
                _singleKeyInputFunc,
                _functionName,
                cache,
                timeToLiveFactory,
                keySerializer,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onException,
                keyComparer);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }
        
        private protected EnumerableKeyFunctionCache<TK, TV> BuildEnumerableKeyFunction()
        {
            if (!_isEnumerableKey)
                throw new Exception($"You can't build a {nameof(EnumerableKeyFunctionCache<TK, TV>)} since your function is single key");

            var keyComparer = GetKeyComparer(_keyComparers);
            
            var cache = BuildCache(keyComparer);
            
            var keySerializer = GetKeySerializer();

            if (_keysToRemoveObservable != null)
            {
                _keysToRemoveObservable
                    .SelectMany(k => Observable.FromAsync(() => cache.Remove(new Key<TK>(k, keySerializer)).AsTask()))
                    .Retry()
                    .Subscribe();
            }
            
            var functionCache = new EnumerableKeyFunctionCache<TK, TV>(
                _enumerableKeyInputFunc,
                _functionName,
                cache,
                _timeToLive ?? DefaultSettings.Cache.TimeToLive,
                keySerializer,
                _defaultValueFactory,
                _onResult,
                _onFetch,
                _onException,
                keyComparer);
            
            PendingRequestsCounterContainer.Add(functionCache);

            return functionCache;
        }

        private ICacheInternal<TK, TV> BuildCache(KeyComparer<TK> keyComparer)
        {
            var cacheConfig = new DistributedCacheConfig<TK, TV>(_functionName)
            {
                KeyspacePrefix = _keyspacePrefix,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = GetValueSerializer(),
                ValueDeserializer = GetValueDeserializer(),
                KeyComparer = keyComparer
            };

            ICacheInternal<TK, TV> cache = null;
            if (!_disableCache ?? !DefaultSettings.Cache.DisableCache)
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

        protected virtual Func<TK, string> GetKeySerializer()
        {
            return GetKeySerializerImpl<TK>();
        }

        protected Func<T, string> GetKeySerializerImpl<T>()
        {
            if (_keySerializers.TryGetSerializer<T>(out var serializer))
                return serializer;
            
            if (serializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetSerializer(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(T).FullName}'"));
        }

        protected virtual Func<string, TK> GetKeyDeserializer()
        {
            return GetKeyDeserializerImpl<TK>();
        }
        
        protected Func<string, T> GetKeyDeserializerImpl<T>()
        {
            if (_keySerializers.TryGetDeserializer<T>(out var deserializer))
                return deserializer;
            
            if (deserializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetDeserializer(out deserializer);

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(T).FullName}'"));
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

        private protected KeyComparer<TK> GetKeyComparer() => GetKeyComparer(_keyComparers);

        private protected virtual KeyComparer<TK> GetKeyComparer(EqualityComparers comparers)
        {
            return KeyComparerResolver.Get<TK>(comparers);
        }
    }
}