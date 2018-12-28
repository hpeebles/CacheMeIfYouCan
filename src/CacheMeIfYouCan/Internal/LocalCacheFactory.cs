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
    internal class LocalCacheFactory : ILocalCacheFactory
    {
        private readonly ILocalCacheFactory _cacheFactory;
        private readonly KeySerializers _keySerializers = new KeySerializers();
        private readonly List<ILocalCacheWrapperFactory> _wrapperFactories;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        public LocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
            _wrapperFactories = new List<ILocalCacheWrapperFactory>();
        }

        public LocalCacheFactory OnGetResult(
            Action<CacheGetResult> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, behaviour);
            return this;
        }

        public LocalCacheFactory OnSetResult(
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, behaviour);
            return this;
        }

        public LocalCacheFactory OnError(
            Action<CacheException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, behaviour);
            return this;
        }

        public LocalCacheFactory WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(_keySerializers);
            return this;
        }
        
        public LocalCacheFactory WithWrapper(
            ILocalCacheWrapperFactory wrapperFactory,
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
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            var cache = _cacheFactory.Build<TK, TV>(cacheName);

            // First wrapper formats any exceptions
            cache = new LocalCacheExceptionFormattingWrapper<TK, TV>(cache);
            
            // Next apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Last wrapper handles notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onError != null)
                cache = new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);

            return cache;
        }

        public ICache<TK, TV> BuildAsCache<TK, TV>(string cacheName)
        {
            var cache = Build<TK, TV>(cacheName);

            _keySerializers.TryGetSerializer<TK>(out var serializer);
            
            return new LocalCacheToCacheAdapter<TK, TV>(cache, serializer);
        }
    }
    
    public class LocalCacheFactory<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory<TK, TV> _cacheFactory;
        private readonly List<ILocalCacheWrapperFactory<TK, TV>> _wrapperFactories;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;
        private Func<TK, string> _keySerializer;
        
        internal LocalCacheFactory(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory;
            _wrapperFactories = new List<ILocalCacheWrapperFactory<TK, TV>>();
        }

        public LocalCacheFactory<TK, TV> OnGetResult(
            Action<CacheGetResult<TK, TV>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, behaviour);
            return this;
        }

        public LocalCacheFactory<TK, TV> OnSetResult(
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, behaviour);
            return this;
        }

        public LocalCacheFactory<TK, TV> OnError(
            Action<CacheException<TK>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, behaviour);
            return this;
        }
        
        public LocalCacheFactory<TK, TV> WithKeySerializer(ISerializer<TK> serializer)
        {
            return WithKeySerializer(serializer.Serialize);
        }

        public LocalCacheFactory<TK, TV> WithKeySerializer(Func<TK, string> serializer)
        {
            _keySerializer = serializer;
            return this;
        }
        
        public LocalCacheFactory<TK, TV> WithWrapper(
            ILocalCacheWrapperFactory<TK, TV> wrapperFactory,
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
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            var cache = _cacheFactory.Build(cacheName);

            // First wrapper formats any exceptions
            cache = new LocalCacheExceptionFormattingWrapper<TK, TV>(cache);
            
            // Next apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Last wrapper handles notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onError != null)
                cache = new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);

            return cache;
        }

        public ICache<TK, TV> BuildAsCache(string cacheName)
        {
            var cache = Build(cacheName);

            return new LocalCacheToCacheAdapter<TK, TV>(cache, _keySerializer);
        }
    }
    
    internal class LocalCacheToCacheAdapter<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Func<TK, string> _keySerializer;

        public LocalCacheToCacheAdapter(ILocalCache<TK, TV> cache, Func<TK, string> keySerializer)
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

        public Task<TV> Get(TK key)
        {
            var fromCache = _cache.Get(new Key<TK>(key, _keySerializer));

            return Task.FromResult(fromCache.Value);
        }

        public Task Set(TK key, TV value, TimeSpan timeToLive)
        {
            _cache.Set(BuildKey(key), value, timeToLive);
            
            return Task.CompletedTask;
        }

        public Task<IDictionary<TK, TV>> Get(ICollection<TK> keys)
        {
            var fromCache = _cache.Get(keys.Select(BuildKey).ToArray());

            IDictionary<TK, TV> results = fromCache.ToDictionary(r => r.Key.AsObject, r => r.Value);

            return Task.FromResult(results);
        }

        public Task Set(ICollection<KeyValuePair<TK, TV>> values, TimeSpan timeToLive)
        {
            var forCache = values
                .Select(kv => new KeyValuePair<Key<TK>, TV>(BuildKey(kv.Key), kv.Value))
                .ToArray();
            
            _cache.Set(forCache, timeToLive);

            return Task.CompletedTask;
        }

        private Key<TK> BuildKey(TK key)
        {
            return new Key<TK>(key, _keySerializer);
        }
    }
}