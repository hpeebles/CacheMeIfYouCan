using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
        where TConfig : FunctionCacheConfigurationManagerBase<TConfig, TK, TV>
    {
        internal KeySerializers KeySerializers { get; }
        internal EqualityComparers KeyComparers { get; }
        internal string Name { get; private set; }
        internal TimeSpan? TimeToLive { get; set; }
        internal TimeSpan? LocalCacheTimeToLiveOverride { get; set; }
        internal bool? Disabled { get; private set; }
        internal bool? DuplicateRequestCatchingEnabled { get; private set; }
        internal Action<FunctionCacheGetResult<TK, TV>> OnResultAction { get; private set; }
        internal Action<FunctionCacheFetchResult<TK, TV>> OnFetchAction { get; private set; }
        internal Action<FunctionCacheException<TK>> OnExceptionAction { get; private set; }
        internal Action<CacheGetResult<TK, TV>> OnCacheGetAction { get; private set; }
        internal Action<CacheSetResult<TK, TV>> OnCacheSetAction { get; private set; }
        internal Action<CacheRemoveResult<TK>> OnCacheRemoveAction { get; private set; }
        internal Action<CacheException<TK>> OnCacheExceptionAction { get; private set; }
        internal Func<TV, string> ValueSerializer { get; private set; }
        internal Func<string, TV> ValueDeserializer { get; private set; }
        internal Func<TV, byte[]> ValueByteSerializer { get; private set; }
        internal Func<byte[], TV> ValueByteDeserializer { get; private set; }
        internal ILocalCacheFactory<TK, TV> LocalCacheFactory { get; private set; }
        internal IDistributedCacheFactory<TK, TV> DistributedCacheFactory { get; private set; }
        internal string KeyspacePrefix { get; private set; }
        internal Func<TV> DefaultValueFactory { get; private set; }
        internal Func<TK, bool> SkipCacheGetPredicate { get; private set; }
        internal Func<TK, bool> SkipCacheSetPredicate { get; private set; }
        internal List<(IObservable<TK> keysToRemove, bool removeFromLocalOnly)> KeyRemovalObservables { get; }
            = new List<(IObservable<TK>, bool)>();
        
        internal FunctionCacheConfigurationManagerBase(
            string name,
            CachedProxyConfig interfaceConfig = null,
            CachedProxyFunctionInfo proxyFunctionInfo = null)
        {
            Name = name;
            
            if (interfaceConfig != null && proxyFunctionInfo != null)
            {
                KeySerializers = interfaceConfig.KeySerializers.Clone();
                KeyComparers = interfaceConfig.KeyComparers.Clone();
                
                if (interfaceConfig.ValueSerializers.TryGetByteSerializer<TV>(out var valueByteSerializer))
                    ValueByteSerializer = valueByteSerializer;
                else if (interfaceConfig.ValueSerializers.TryGetSerializer<TV>(out var valueSerializer))
                    ValueSerializer = valueSerializer;

                if (interfaceConfig.ValueSerializers.TryGetByteDeserializer<TV>(out var valueByteDeserializer))
                    ValueByteDeserializer = valueByteDeserializer;
                else if (interfaceConfig.ValueSerializers.TryGetDeserializer<TV>(out var valueDeserializer))
                    ValueDeserializer = valueDeserializer;

                if (interfaceConfig.NameGenerator != null)
                    Name = interfaceConfig.NameGenerator(proxyFunctionInfo.MethodInfo);
                
                TimeToLive = interfaceConfig.TimeToLive;
                LocalCacheTimeToLiveOverride = interfaceConfig.LocalCacheTimeToLiveOverride;
                Disabled = interfaceConfig.DisableCache;
                DuplicateRequestCatchingEnabled = interfaceConfig.CatchDuplicateRequests;
                
                if (interfaceConfig.LocalCacheFactory is NullLocalCacheFactory)
                    LocalCacheFactory = new NullLocalCacheFactory<TK, TV>();
                else if (interfaceConfig.LocalCacheFactory != null)
                    LocalCacheFactory = new LocalCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.LocalCacheFactory);
                
                if (interfaceConfig.DistributedCacheFactory is NullDistributedCacheFactory)
                    DistributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();
                else if (interfaceConfig.DistributedCacheFactory != null)
                    DistributedCacheFactory = new DistributedCacheFactoryToGenericAdapter<TK, TV>(interfaceConfig.DistributedCacheFactory);

                KeyspacePrefix = interfaceConfig.KeyspacePrefixFunc?.Invoke(proxyFunctionInfo.MethodInfo);
                OnResultAction = interfaceConfig.OnResult;
                OnFetchAction = interfaceConfig.OnFetch;
                OnExceptionAction = interfaceConfig.OnException;
                OnCacheGetAction = interfaceConfig.OnCacheGet;
                OnCacheSetAction = interfaceConfig.OnCacheSet;
                OnCacheRemoveAction = interfaceConfig.OnCacheRemove;
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

        public TConfig Named(string name)
        {
            Name = name;
            return (TConfig)this;
        }

        public TConfig WithLocalCacheTimeToLiveOverride(TimeSpan? timeToLive)
        {
            LocalCacheTimeToLiveOverride = timeToLive;
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
            ValueByteSerializer = null;
            ValueByteDeserializer = null;
            return (TConfig)this;
        }
        
        public TConfig WithValueSerializer(IByteSerializer serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize<TV>);
        }
        
        public TConfig WithValueSerializer(IByteSerializer<TV> serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        public TConfig WithValueSerializer(Func<TV, byte[]> serializer, Func<byte[], TV> deserializer)
        {
            ValueByteSerializer = serializer;
            ValueByteDeserializer = deserializer;
            ValueSerializer = null;
            ValueDeserializer = null;
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

        public TConfig CatchDuplicateRequests(bool catchDuplicateRequests = true)
        {
            DuplicateRequestCatchingEnabled = catchDuplicateRequests;
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
        
        public TConfig WithLocalCacheFactory(Func<ILocalCacheConfig<TK>, ILocalCache<TK, TV>> cacheFactoryFunc)
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
        
        public TConfig WithMemoryCache()
        {
            return WithLocalCacheFactory(new MemoryCacheFactory());
        }

        public TConfig WithDictionaryCache()
        {
            return WithLocalCacheFactory(new DictionaryCacheFactory());
        }

        public TConfig WithRollingTimeToLiveDictionaryCache(TimeSpan rollingTimeToLive)
        {
            return WithLocalCacheFactory(new RollingTimeToLiveDictionaryCacheFactory(rollingTimeToLive));
        }

        public TConfig WithDistributedCache(IDistributedCache<TK, TV> cache)
        {
            return WithDistributedCacheFactory(c => cache);
        }
        
        public TConfig WithDistributedCacheFactory(IDistributedCacheFactory cacheFactory)
        {
            return WithDistributedCacheFactory(cacheFactory.Build);
        }

        public TConfig WithDistributedCacheFactory(IDistributedCacheFactory<TK, TV> cacheFactory)
        {
            return WithDistributedCacheFactory(cacheFactory.Build);
        }
        
        public TConfig WithDistributedCacheFactory(Func<IDistributedCacheConfig<TK, TV>, IDistributedCache<TK, TV>> cacheFactoryFunc)
        {
            DistributedCacheFactory = new DistributedCacheFactoryFromFuncAdapter<TK, TV>(cacheFactoryFunc);
            return (TConfig)this;
        }

        public TConfig WithKeyspacePrefix(string keyspacePrefix)
        {
            KeyspacePrefix = keyspacePrefix;
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCache(bool skipDistributedCache = true)
        {
            if (skipDistributedCache)
                DistributedCacheFactory = new NullDistributedCacheFactory<TK, TV>();

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
        
        public TConfig OnCacheRemove(
            Action<CacheRemoveResult<TK>> onCacheRemove,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheRemoveAction = ActionsHelper.Combine(OnCacheRemoveAction, onCacheRemove, behaviour);
            return (TConfig)this;
        }

        public TConfig OnCacheException(
            Action<CacheException<TK>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheExceptionAction = ActionsHelper.Combine(OnCacheExceptionAction, onCacheException, behaviour);
            return (TConfig)this;
        }
        
        public TConfig OnResultObservable(
            Action<IObservable<FunctionCacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, OnResult, behaviour);
        }
        
        public TConfig OnFetchObservable(
            Action<IObservable<FunctionCacheFetchResult<TK, TV>>> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onFetch, OnFetch, behaviour);
        }
        
        public TConfig OnExceptionObservable(
            Action<IObservable<FunctionCacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, OnException, behaviour);
        }
        
        public TConfig OnCacheGetObservable(
            Action<IObservable<CacheGetResult<TK, TV>>> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheGet, OnCacheGet, behaviour);
        }
        
        public TConfig OnCacheSetObservable(
            Action<IObservable<CacheSetResult<TK, TV>>> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheSet, OnCacheSet, behaviour);
        }
        
        public TConfig OnCacheExceptionObservable(
            Action<IObservable<CacheException<TK>>> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onCacheException, OnCacheException, behaviour);
        }

        public TConfig SkipCacheWhen(Func<TK, bool> predicate, SkipCacheSettings settings = SkipCacheSettings.SkipGetAndSet)
        {
            if (settings.HasFlag(SkipCacheSettings.SkipGet))
            {
                var current = SkipCacheGetPredicate;
                if (current == null)
                    SkipCacheGetPredicate = predicate;
                else
                    SkipCacheGetPredicate = k => current(k) || predicate(k);
            }
            
            if (settings.HasFlag(SkipCacheSettings.SkipSet))
            {
                var current = SkipCacheSetPredicate;
                if (current == null)
                    SkipCacheSetPredicate = predicate;
                else
                    SkipCacheSetPredicate = k => current(k) || predicate(k);
            }

            return (TConfig)this;
        }

        public TConfig WithKeysToRemoveObservable(IObservable<TK> keysToRemoveObservable, bool removeFromLocalOnly = false)
        {
            KeyRemovalObservables.Add((keysToRemoveObservable, removeFromLocalOnly));
            return (TConfig)this;
        }
        
        internal ICacheInternal<TK, TV> BuildCache(Func<TK, string> keySerializer, KeyComparer<TK> keyComparer)
        {
            var cacheConfig = new DistributedCacheConfig<TK, TV>(Name)
            {
                KeyspacePrefix = KeyspacePrefix,
                KeySerializer = keySerializer,
                KeyDeserializer = GetKeyDeserializer(),
                ValueSerializer = ValueSerializer,
                ValueDeserializer = ValueDeserializer,
                ValueByteSerializer = ValueByteSerializer,
                ValueByteDeserializer = ValueByteDeserializer,
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

                var keyRemovalObservables = KeyRemovalObservables
                    .Select(t => (t.keysToRemove.Select(k => new Key<TK>(k, keySerializer)), t.removeFromLocalOnly))
                    .ToList();

                cache = CacheBuilder.Build(
                    localCacheFactory,
                    distributedCacheFactory,
                    cacheConfig,
                    OnCacheGetAction,
                    OnCacheSetAction,
                    OnCacheRemoveAction,
                    OnCacheExceptionAction,
                    keyRemovalObservables,
                    LocalCacheTimeToLiveOverride);
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

        internal virtual KeyComparer<TK> GetKeyComparer()
        {
            throw new NotImplementedException();
        }
    }
}