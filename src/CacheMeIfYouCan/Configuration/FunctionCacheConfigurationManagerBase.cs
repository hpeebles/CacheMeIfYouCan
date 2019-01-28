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
        internal string FunctionName { get; }
        internal KeySerializers KeySerializers { get; }
        internal EqualityComparers KeyComparers { get; }
        internal TimeSpan? TimeToLive { get; set; }
        internal bool? Disabled { get; private set; }
        internal Action<FunctionCacheGetResult<TK, TV>> OnResultAction { get; private set; }
        internal Action<FunctionCacheFetchResult<TK, TV>> OnFetchAction { get; private set; }
        internal Action<FunctionCacheException<TK>> OnExceptionAction { get; private set; }
        internal Action<CacheGetResult<TK, TV>> OnCacheGetAction { get; private set; }
        internal Action<CacheSetResult<TK, TV>> OnCacheSetAction { get; private set; }
        internal Action<CacheException<TK>> OnCacheExceptionAction { get; private set; }
        internal Func<TV, string> ValueSerializer { get; private set; }
        internal Func<string, TV> ValueDeserializer { get; private set; }
        internal ILocalCacheFactory<TK, TV> LocalCacheFactory { get; private set; }
        internal IDistributedCacheFactory<TK, TV> DistributedCacheFactory { get; private set; }
        internal string KeyspacePrefix { get; private set; }
        internal Func<TV> DefaultValueFactory { get; private set; }
        internal IObservable<TK> KeysToRemoveObservable { get; private set; }
        
        internal FunctionCacheConfigurationManagerBase(
            string functionName,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
        {
            FunctionName = functionName;

            if (interfaceConfig != null)
            {
                KeySerializers = interfaceConfig.KeySerializers.Clone();
                KeyComparers = interfaceConfig.KeyComparers.Clone();

                if (interfaceConfig.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer))
                    ValueSerializer = valueSerializer;

                if (interfaceConfig.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer))
                    ValueDeserializer = valueDeserializer;

                TimeToLive = interfaceConfig.TimeToLive;
                Disabled = interfaceConfig.DisableCache;

                if (interfaceConfig.LocalCacheFactory is NullLocalCacheFactory)
                    LocalCacheFactory = new NullLocalCacheFactory<TK, TV>();
                else if (interfaceConfig.LocalCacheFactory != null)
                    LocalCacheFactory = new LocalCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.LocalCacheFactory);
                
                if (interfaceConfig.DistributedCacheFactory is NullDistributedCacheFactory)
                    DistributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();
                else if (interfaceConfig.DistributedCacheFactory != null)
                    DistributedCacheFactory = new DistributedCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.DistributedCacheFactory);

                KeyspacePrefix = interfaceConfig.KeyspacePrefixFunc?.Invoke(proxyFunctionInfo);
                OnResultAction = interfaceConfig.OnResult;
                OnFetchAction = interfaceConfig.OnFetch;
                OnExceptionAction = interfaceConfig.OnException;
                OnCacheGetAction = interfaceConfig.OnCacheGet;
                OnCacheSetAction = interfaceConfig.OnCacheSet;
                OnCacheExceptionAction = interfaceConfig.OnCacheException;

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
                KeySerializers = new KeySerializers();
                KeyComparers = new EqualityComparers();
                OnResultAction = DefaultSettings.Cache.OnResultAction;
                OnFetchAction = DefaultSettings.Cache.OnFetchAction;
                OnExceptionAction = DefaultSettings.Cache.OnExceptionAction;
                OnCacheGetAction = DefaultSettings.Cache.OnCacheGetAction;
                OnCacheSetAction = DefaultSettings.Cache.OnCacheSetAction;
                OnCacheExceptionAction = DefaultSettings.Cache.OnCacheExceptionAction;
            }
        }

        public virtual TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            TimeToLive = timeToLive;
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
            return WithKeySerializerInternal(serializer, deserializer);
        }

        internal TConfig WithKeySerializerInternal<T>(ISerializer<T> serializer)
        {
            return WithKeySerializerInternal(serializer.Serialize, serializer.Deserialize);
        }

        internal TConfig WithKeySerializerInternal<T>(Func<T, string> serializer, Func<string, T> deserializer = null)
        {
            KeySerializers.Set(serializer, deserializer);
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
            ValueSerializer = serializer;
            ValueDeserializer = deserializer;
            return (TConfig)this;
        }

        internal TConfig WithKeyComparerInternal<T>(IEqualityComparer<T> comparer)
        {
            KeyComparers.Set(comparer);
            return (TConfig)this;
        }

        public TConfig DisableCache(bool disableCache = true)
        {
            Disabled = disableCache;
            return (TConfig)this;
        }
        
        public TConfig ContinueOnException(TV defaultValue = default)
        {
            return ContinueOnException(() => defaultValue);
        }

        public TConfig ContinueOnException(Func<TV> defaultValueFactory)
        {
            DefaultValueFactory = defaultValueFactory;
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
            LocalCacheFactory = new LocalCacheFactoryFromFuncAdapter<TK, TV>(cacheFactoryFunc);
            return (TConfig)this;
        }
        
        public TConfig SkipLocalCache(bool skipLocalCache = true)
        {
            if (skipLocalCache)
                LocalCacheFactory = new NullLocalCacheFactory<TK, TV>();
            
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
            DistributedCacheFactory = new DistributedCacheFactoryFromFuncAdapter<TK, TV>(cacheFactoryFunc);
            KeyspacePrefix = keyspacePrefix;
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCache(bool skipDistributedCache = true)
        {
            if (skipDistributedCache)
            {
                DistributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();
                KeyspacePrefix = null;
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
            OnResultAction = ActionsHelper.Combine(OnResultAction, onResult, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnFetch(
            Action<FunctionCacheFetchResult<TK, TV>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnFetchAction = ActionsHelper.Combine(OnFetchAction, onFetch, behaviour);
            return (TConfig)this;
        }

        public TConfig OnException(
            Action<FunctionCacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnExceptionAction = ActionsHelper.Combine(OnExceptionAction, onException, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnCacheGet(
            Action<CacheGetResult<TK, TV>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheGetAction = ActionsHelper.Combine(OnCacheGetAction, onCacheGet, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnCacheSet(
            Action<CacheSetResult<TK, TV>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheSetAction = ActionsHelper.Combine(OnCacheSetAction, onCacheSet, behaviour);
            return (TConfig)this;
        }

        public TConfig OnCacheException(
            Action<CacheException<TK>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheExceptionAction = ActionsHelper.Combine(OnCacheExceptionAction, onCacheException, behaviour);
            return (TConfig)this;
        }

        public TConfig WithKeysToRemoveObservable(IObservable<TK> keysToRemoveObservable)
        {
            KeysToRemoveObservable = keysToRemoveObservable;
            return (TConfig)this;
        }
        
        internal ICacheInternal<TK, TV> BuildCache(KeyComparer<TK> keyComparer)
        {
            var cacheConfig = new DistributedCacheConfig<TK, TV>(FunctionName)
            {
                KeyspacePrefix = KeyspacePrefix,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = GetValueSerializer(),
                ValueDeserializer = GetValueDeserializer(),
                KeyComparer = keyComparer
            };

            ICacheInternal<TK, TV> cache = null;
            if (!Disabled ?? !DefaultSettings.Cache.DisableCache)
            {
                ILocalCacheFactory<TK, TV> localCacheFactory = null;
                if (LocalCacheFactory != null)
                    localCacheFactory = LocalCacheFactory;
                else if (DefaultSettings.Cache.LocalCacheFactory != null)
                    localCacheFactory = new LocalCacheFactoryToGenericAdapter<TK, TV>(DefaultSettings.Cache.LocalCacheFactory);

                IDistributedCacheFactory<TK, TV> distributedCacheFactory = null;
                if (DistributedCacheFactory != null)
                    distributedCacheFactory = DistributedCacheFactory;
                else if (DefaultSettings.Cache.DistributedCacheFactory != null)
                    distributedCacheFactory = new DistributedCacheFactoryToGenericAdapter<TK, TV>(DefaultSettings.Cache.DistributedCacheFactory);

                cache = CacheBuilder.Build(
                    FunctionName,
                    localCacheFactory,
                    distributedCacheFactory,
                    cacheConfig,
                    OnCacheGetAction,
                    OnCacheSetAction,
                    OnCacheExceptionAction,
                    keyComparer);
            }

            return cache;
        }

        internal virtual Func<TK, string> GetKeySerializer()
        {
            return GetKeySerializerImpl<TK>();
        }

        internal Func<T, string> GetKeySerializerImpl<T>()
        {
            if (KeySerializers.TryGetSerializer<T>(out var serializer))
                return serializer;
            
            if (serializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetSerializer(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No key serializer defined for type '{typeof(T).FullName}'"));
        }

        internal virtual Func<string, TK> GetKeyDeserializer()
        {
            return GetKeyDeserializerImpl<TK>();
        }
        
        internal Func<string, T> GetKeyDeserializerImpl<T>()
        {
            if (KeySerializers.TryGetDeserializer<T>(out var deserializer))
                return deserializer;
            
            if (deserializer == null)
                DefaultSettings.Cache.KeySerializers.TryGetDeserializer(out deserializer);

            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No key deserializer defined for type '{typeof(T).FullName}'"));
        }
        
        private Func<TV, string> GetValueSerializer()
        {
            var serializer = ValueSerializer;

            if (serializer == null)
                DefaultSettings.Cache.ValueSerializers.TryGetSerializer<TV>(out serializer);

            if (serializer == null)
                ProvidedSerializers.TryGetSerializer(out serializer);

            return serializer ?? (_ => throw new Exception($"No value serializer defined for type '{typeof(TV).FullName}'"));
        }
        
        private Func<string, TV> GetValueDeserializer()
        {
            var deserializer = ValueDeserializer;
            
            if (deserializer == null)
                DefaultSettings.Cache.ValueSerializers.TryGetDeserializer<TV>(out deserializer);
            
            if (deserializer == null)
                ProvidedSerializers.TryGetDeserializer(out deserializer);

            return deserializer ?? (_ => throw new Exception($"No value deserializer defined for type '{typeof(TV).FullName}'"));
        }

        internal virtual KeyComparer<TK> GetKeyComparer()
        {
            return KeyComparerResolver.Get<TK>(KeyComparers);
        }
    }
}