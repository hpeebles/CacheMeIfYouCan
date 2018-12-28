using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal
{
    internal class DistributedCacheFactory : IDistributedCacheFactory
    {
        private readonly IDistributedCacheFactory _cacheFactory;
        private readonly KeySerializers _keySerializers;
        private readonly ValueSerializers _valueSerializers;
        private readonly List<IDistributedCacheWrapperFactory> _wrapperFactories;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;
        private Func<string, string> _keyspacePrefixFunc;

        public DistributedCacheFactory(IDistributedCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
            _keySerializers = new KeySerializers();
            _valueSerializers = new ValueSerializers();
            _wrapperFactories = new List<IDistributedCacheWrapperFactory>();
        }

        public DistributedCacheFactory OnGetResult(
            Action<CacheGetResult> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, behaviour);
            return this;
        }

        public DistributedCacheFactory OnSetResult(
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, behaviour);
            return this;
        }

        public DistributedCacheFactory OnError(
            Action<CacheException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, behaviour);
            return this;
        }
        
        public DistributedCacheFactory WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(_keySerializers);
            return this;
        }
        
        public DistributedCacheFactory WithValueSerializers(Action<ValueSerializers> configAction)
        {
            configAction(_valueSerializers);
            return this;
        }
        
        public DistributedCacheFactory WithKeyspacePrefix(string keyspacePrefix)
        {
            return WithKeyspacePrefix(c => keyspacePrefix);
        }
        
        public DistributedCacheFactory WithKeyspacePrefix(Func<string, string> keyspacePrefixFunc)
        {
            _keyspacePrefixFunc = keyspacePrefixFunc;
            return this;
        }
        
        public DistributedCacheFactory WithWrapper(
            IDistributedCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour)
        {
            switch (behaviour)
            {
                case AdditionBehaviour.Append:
                    _wrapperFactories.Add(wrapperFactory);
                    break;

                case AdditionBehaviour.Prepend:
                    _wrapperFactories.Insert(0, wrapperFactory);
                    break;
                
                case AdditionBehaviour.Overwrite:
                    _wrapperFactories.Clear();
                    _wrapperFactories.Add(wrapperFactory);
                    break;
            }
            
            return this;
        }

        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            // First wrapper formats any exceptions
            cache = new DistributedCacheExceptionFormattingWrapper<TK, TV>(cache);
            
            // Next apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Last wrapper handles notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onError != null)
                cache = new DistributedCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);

            return cache;
        }

        public IDistributedCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            var config = new DistributedCacheConfig<TK, TV>(cacheName);

            if (_keyspacePrefixFunc != null)
                config.KeyspacePrefix = _keyspacePrefixFunc(cacheName);
            
            if (_keySerializers.TryGetDeserializer<TK>(out var keyDeserializer))
                config.KeyDeserializer = keyDeserializer;

            if (_valueSerializers.TryGetSerializer<TV>(out var valueSerializer))
                config.ValueSerializer = valueSerializer;

            if (_valueSerializers.TryGetDeserializer<TV>(out var valueDeserializer))
                config.ValueDeserializer = valueDeserializer;
            
            return Build(config);
        }
        
        public ICache<TK, TV> BuildAsCache<TK, TV>(string cacheName)
        {
            var cache = Build<TK, TV>(cacheName);
            
            _keySerializers.TryGetSerializer<TK>(out var keySerializer);
            
            return new DistributedCacheToCacheAdaptor<TK, TV>(cache, keySerializer);
        }
    }
    
    public class DistributedCacheFactory<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly IDistributedCacheFactory<TK, TV> _cacheFactory;
        private readonly List<IDistributedCacheWrapperFactory<TK, TV>> _wrapperFactories;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;
        private Func<TK, string> _keySerializer;
        private Func<string, TK> _keyDeserializer;
        private Func<TV, string> _valueSerializer;
        private Func<string, TV> _valueDeserializer;
        private Func<string, string> _keyspacePrefixFunc;
        
        internal DistributedCacheFactory(IDistributedCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
            _wrapperFactories = new List<IDistributedCacheWrapperFactory<TK, TV>>();
        }

        public DistributedCacheFactory<TK, TV> OnGetResult(
            Action<CacheGetResult<TK, TV>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, behaviour);
            return this;
        }

        public DistributedCacheFactory<TK, TV> OnSetResult(
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, behaviour);
            return this;
        }

        public DistributedCacheFactory<TK, TV> OnError(
            Action<CacheException<TK>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, behaviour);
            return this;
        }

        public DistributedCacheFactory<TK, TV> WithKeySerializer(ISerializer<TK> serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        public DistributedCacheFactory<TK, TV> WithKeySerializer(ISerializer serializer)
        {
            return WithKeySerializer(serializer.Serialize, serializer.Deserialize<TK>);
        }

        public DistributedCacheFactory<TK, TV> WithKeySerializer(
            Func<TK, string> serializer,
            Func<string, TK> deserializer)
        {
            _keySerializer = serializer;
            _keyDeserializer = deserializer;
            return this;
        }

        public DistributedCacheFactory<TK, TV> WithValueSerializer(ISerializer<TV> serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize);
        }

        public DistributedCacheFactory<TK, TV> WithValueSerializer(ISerializer serializer)
        {
            return WithValueSerializer(serializer.Serialize, serializer.Deserialize<TV>);
        }

        public DistributedCacheFactory<TK, TV> WithValueSerializer(
            Func<TV, string> serializer,
            Func<string, TV> deserializer)
        {
            _valueSerializer = serializer;
            _valueDeserializer = deserializer;
            return this;
        }
        
        public DistributedCacheFactory<TK, TV> WithKeyspacePrefix(Func<string, string> keyspacePrefixFunc)
        {
            _keyspacePrefixFunc = keyspacePrefixFunc;
            return this;
        }

        public DistributedCacheFactory<TK, TV> WithWrapper(
            IDistributedCacheWrapperFactory<TK, TV> wrapperFactory,
            AdditionBehaviour behaviour)
        {
            switch (behaviour)
            {
                case AdditionBehaviour.Append:
                    _wrapperFactories.Add(wrapperFactory);
                    break;

                case AdditionBehaviour.Prepend:
                    _wrapperFactories.Insert(0, wrapperFactory);
                    break;
                
                case AdditionBehaviour.Overwrite:
                    _wrapperFactories.Clear();
                    _wrapperFactories.Add(wrapperFactory);
                    break;
            }
            
            return this;
        }

        public IDistributedCache<TK, TV> Build(DistributedCacheConfig<TK, TV> config)
        {
            var cacheConfig = new DistributedCacheConfig<TK, TV>
            {
                KeyspacePrefix = config.KeyspacePrefix,
                KeyDeserializer = _keyDeserializer ?? config.KeyDeserializer,
                ValueSerializer = _valueSerializer ?? config.ValueSerializer,
                ValueDeserializer = _valueDeserializer ?? config.ValueDeserializer
            };
            
            var cache = _cacheFactory.Build(cacheConfig);

            // First wrapper formats any exceptions
            cache = new DistributedCacheExceptionFormattingWrapper<TK, TV>(cache);
            
            // Next apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Last wrapper handles notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onError != null)
                cache = new DistributedCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);

            return cache;
        }

        internal IDistributedCache<TK, TV> Build(string cacheName)
        {
            var config = new DistributedCacheConfig<TK, TV>(cacheName);

            if (_keyspacePrefixFunc != null)
                config.KeyspacePrefix = _keyspacePrefixFunc(cacheName);
            
            if (_keyDeserializer != null)
                config.KeyDeserializer = _keyDeserializer;

            if (_valueSerializer != null)
                config.ValueSerializer = _valueSerializer;

            if (_valueDeserializer != null)
                config.ValueDeserializer = _valueDeserializer;
            
            return Build(config);
        }
        
        internal ICache<TK, TV> BuildAsCache(string cacheName)
        {
            var cache = Build(cacheName);
            
            return new DistributedCacheToCacheAdaptor<TK, TV>(cache, _keySerializer);
        }
    }

    internal class DistributedCacheToCacheAdaptor<TK, TV> : ICache<TK, TV>
    {
        private readonly IDistributedCache<TK, TV> _cache;
        private readonly Func<TK, string> _keySerializer;

        public DistributedCacheToCacheAdaptor(IDistributedCache<TK, TV> cache, Func<TK, string> keySerializer)
        {
            _cache = cache;
            
            if (keySerializer != null)
            {
                _keySerializer = keySerializer;
            }
            else if (
                DefaultCacheConfig.Configuration.KeySerializers.TryGetSerializer<TK>(out var s) ||
                ProvidedSerializers.TryGetSerializer(out s))
            {
                _keySerializer = s;
            }
        }

        public async Task<TV> Get(TK key)
        {
            var fromCache = await _cache.Get(new Key<TK>(key, _keySerializer));

            return fromCache.Value;
        }

        public async Task Set(TK key, TV value, TimeSpan timeToLive)
        {
            await _cache.Set(BuildKey(key), value, timeToLive);
        }

        public async Task<IDictionary<TK, TV>> Get(ICollection<TK> keys)
        {
            var fromCache = await _cache.Get(keys.Select(BuildKey).ToArray());

            return fromCache.ToDictionary(r => r.Key.AsObject, r => r.Value);
        }

        public async Task Set(ICollection<KeyValuePair<TK, TV>> values, TimeSpan timeToLive)
        {
            var forCache = values
                .Select(kv => new KeyValuePair<Key<TK>, TV>(BuildKey(kv.Key), kv.Value))
                .ToArray();
            
            await _cache.Set(forCache, timeToLive);
        }

        private Key<TK> BuildKey(TK key)
        {
            return new Key<TK>(key, _keySerializer);
        }
    }
}