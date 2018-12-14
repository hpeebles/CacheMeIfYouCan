using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    internal class LocalCacheFactory : ILocalCacheFactory
    {
        private readonly ILocalCacheFactory _cacheFactory;
        private readonly KeySerializers _keySerializers = new KeySerializers();
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        public LocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;

        public LocalCacheFactory OnGetResult(
            Action<CacheGetResult> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, ordering);
            return this;
        }

        public LocalCacheFactory OnSetResult(
            Action<CacheSetResult> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, ordering);
            return this;
        }

        public LocalCacheFactory OnError(
            Action<CacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, ordering);
            return this;
        }

        public LocalCacheFactory WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(_keySerializers);
            return this;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            var cache = _cacheFactory.Build<TK ,TV>(cacheName);

            return new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);
        }

        public ICache<TK, TV> BuildAsCache<TK, TV>(string cacheName)
        {
            var cache = Build<TK, TV>(cacheName);

            _keySerializers.TryGetSerializer<TK>(out var serializer);
            
            return new LocalCacheAdaptor<TK, TV>(cache, serializer);
        }
    }
    
    public class LocalCacheFactory<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory<TK, TV> _cacheFactory;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;
        private Func<TK, string> _keySerializer;
        
        internal LocalCacheFactory(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;

        public LocalCacheFactory<TK, TV> OnGetResult(
            Action<CacheGetResult<TK, TV>> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, ordering);
            return this;
        }

        public LocalCacheFactory<TK, TV> OnSetResult(
            Action<CacheSetResult<TK, TV>> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, ordering);
            return this;
        }

        public LocalCacheFactory<TK, TV> OnError(
            Action<CacheException<TK>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, ordering);
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

        public ILocalCache<TK, TV> Build(string cacheName)
        {
            var cache = _cacheFactory.Build(cacheName);

            return new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError);
        }

        public ICache<TK, TV> BuildAsCache(string cacheName)
        {
            var cache = Build(cacheName);

            return new LocalCacheAdaptor<TK, TV>(cache, _keySerializer);
        }
    }
    
    internal class LocalCacheAdaptor<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Func<TK, string> _keySerializer;

        public LocalCacheAdaptor(ILocalCache<TK, TV> cache, Func<TK, string> keySerializer)
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