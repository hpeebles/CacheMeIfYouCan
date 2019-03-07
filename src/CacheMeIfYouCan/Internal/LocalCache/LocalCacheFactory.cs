using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Internal.LocalCache
{
    internal class LocalCacheFactory : ILocalCacheFactory
    {
        private readonly ILocalCacheFactory _cacheFactory;
        private readonly KeySerializers _keySerializers;
        private readonly EqualityComparers _keyComparers;
        private readonly List<ILocalCacheWrapperFactory> _wrapperFactories;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheRemoveResult> _onRemoveResult;
        private Action<CacheException> _onException;
        private Func<Exception, bool> _swallowExceptionsPredicate;

        public LocalCacheFactory(ILocalCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
            _keySerializers = new KeySerializers();
            _keyComparers = new EqualityComparers();
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
        
        public LocalCacheFactory OnRemoveResult(
            Action<CacheRemoveResult> onRemoveResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onRemoveResult = ActionsHelper.Combine(_onRemoveResult, onRemoveResult, behaviour);
            return this;
        }

        public LocalCacheFactory OnException(
            Action<CacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onException = ActionsHelper.Combine(_onException, onException, behaviour);
            return this;
        }

        public LocalCacheFactory WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(_keySerializers);
            return this;
        }

        public LocalCacheFactory WithKeyComparer<T>(IEqualityComparer<T> comparer)
        {
            _keyComparers.Set(comparer);
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

        public LocalCacheFactory SwallowExceptions(Func<Exception, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            
            var current = _swallowExceptionsPredicate;

            if (current == null)
                _swallowExceptionsPredicate = predicate;
            else
                _swallowExceptionsPredicate = ex => current(ex) || predicate(ex);

            return this;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config)
        {
            return BuildImpl<TK, TV>(MergeConfigSettings(config));
        }

        internal ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            var config = MergeConfigSettings(new LocalCacheConfig<TK>(cacheName));
            
            return BuildImpl<TK, TV>(config);
        }
        
        internal ICache<TK, TV> BuildAsCache<TK, TV>(string cacheName)
        {
            var config = MergeConfigSettings(new LocalCacheConfig<TK>(cacheName));
            
            var cache = BuildImpl<TK, TV>(config);
            
            return new LocalCacheToCacheAdapter<TK, TV>(cache, config.KeySerializer);
        }

        private ILocalCache<TK, TV> BuildImpl<TK, TV>(ILocalCacheConfig<TK> config)
        {
            var originalCache = _cacheFactory.Build<TK, TV>(config);

            var cache = originalCache;

            // First apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Then add a wrapper to catch and format any exceptions
            cache = new LocalCacheExceptionFormattingWrapper<TK, TV>(cache);

            // Then add a wrapper to handle notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onRemoveResult != null || _onException != null)
                cache = new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onRemoveResult, _onException);

            // Then add a wrapper to swallow exceptions (if required)
            if (_swallowExceptionsPredicate != null)
                cache = new LocalCacheExceptionSwallowingWrapper<TK, TV>(cache, _swallowExceptionsPredicate);

            return new WrappedLocalCacheWithOriginal<TK, TV>(cache, originalCache);
        }

        private LocalCacheConfig<TK> MergeConfigSettings<TK>(ILocalCacheConfig<TK> config)
        {
            // Build the final config by prioritising settings in the following order -
            // Input config -> this LocalCacheFactory settings -> default settings
            var finalConfig = new LocalCacheConfig<TK>(config.CacheName, true);

            if (config.KeySerializer != null)
                finalConfig.KeySerializer = config.KeySerializer;
            else if (_keySerializers.TryGetSerializer<TK>(out var keySerializer))
                finalConfig.KeySerializer = keySerializer;

            if (config.KeyComparer != null)
                finalConfig.KeyComparer = config.KeyComparer;
            else if (_keyComparers.TryGet<TK>(out var comparer))
                finalConfig.KeyComparer = new KeyComparer<TK>(comparer);

            return finalConfig;
        }
    }
    
    internal class LocalCacheFactory<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory<TK, TV> _cacheFactory;
        private readonly List<ILocalCacheWrapperFactory<TK, TV>> _wrapperFactories;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheRemoveResult<TK>> _onRemoveResult;
        private Action<CacheException<TK>> _onException;
        private Func<TK, string> _keySerializer;
        private KeyComparer<TK> _keyComparer;
        private Func<Exception, bool> _swallowExceptionsPredicate;
        
        internal LocalCacheFactory(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
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
        
        public LocalCacheFactory<TK, TV> OnRemoveResult(
            Action<CacheRemoveResult<TK>> onRemoveResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onRemoveResult = ActionsHelper.Combine(_onRemoveResult, onRemoveResult, behaviour);
            return this;
        }

        public LocalCacheFactory<TK, TV> OnException(
            Action<CacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            _onException = ActionsHelper.Combine(_onException, onException, behaviour);
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

        public LocalCacheFactory<TK, TV> WithKeyComparer(IEqualityComparer<TK> comparer)
        {
            _keyComparer = new KeyComparer<TK>(comparer);
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
        
        public LocalCacheFactory<TK, TV> SwallowExceptions(Func<Exception, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            
            var current = _swallowExceptionsPredicate;

            if (current == null)
                _swallowExceptionsPredicate = predicate;
            else
                _swallowExceptionsPredicate = ex => current(ex) || predicate(ex);

            return this;
        }
        
        public ILocalCache<TK, TV> Build(ILocalCacheConfig<TK> config)
        {
            return BuildImpl(MergeConfigSettings(config));
        }

        internal ILocalCache<TK, TV> Build(string cacheName)
        {
            var config = MergeConfigSettings(new LocalCacheConfig<TK>(cacheName));
            
            return BuildImpl(config);
        }
        
        internal ICache<TK, TV> BuildAsCache(string cacheName)
        {
            var config = MergeConfigSettings(new LocalCacheConfig<TK>(cacheName));
            
            var cache = BuildImpl(config);
            
            return new LocalCacheToCacheAdapter<TK, TV>(cache, config.KeySerializer);
        }

        private ILocalCache<TK, TV> BuildImpl(ILocalCacheConfig<TK> config)
        {
            var originalCache = _cacheFactory.Build(config);

            var cache = originalCache;

            // First apply any custom wrappers
            foreach (var wrapperFactory in _wrapperFactories)
                cache = wrapperFactory.Wrap(cache);
            
            // Then add a wrapper to catch and format any exceptions
            cache = new LocalCacheExceptionFormattingWrapper<TK, TV>(cache);

            // Then add a wrapper to handle notifications (if any actions are set)
            if (_onGetResult != null || _onSetResult != null || _onRemoveResult != null || _onException != null)
                cache = new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onRemoveResult, _onException);

            // Then add a wrapper to swallow exceptions (if required)
            if (_swallowExceptionsPredicate != null)
                cache = new LocalCacheExceptionSwallowingWrapper<TK, TV>(cache, _swallowExceptionsPredicate);
            
            return new WrappedLocalCacheWithOriginal<TK, TV>(cache, originalCache);
        }

        private ILocalCacheConfig<TK> MergeConfigSettings(ILocalCacheConfig<TK> config)
        {
            // Build the final config by prioritising settings in the following order -
            // Input config -> this LocalCacheFactory settings -> default settings
            var finalConfig = new LocalCacheConfig<TK>(config.CacheName, true);

            if (config.KeySerializer != null)
                finalConfig.KeySerializer = config.KeySerializer;
            else if (_keySerializer != null)
                finalConfig.KeySerializer = _keySerializer;

            if (config.KeyComparer != null)
                finalConfig.KeyComparer = config.KeyComparer;
            else if (_keyComparer != null)
                finalConfig.KeyComparer = new KeyComparer<TK>(_keyComparer);

            return finalConfig;
        }
    }
    
    internal class LocalCacheToCacheAdapter<TK, TV> : ICache<TK, TV>
    {
        private readonly ILocalCache<TK, TV> _cache;
        private readonly Func<TK, string> _keySerializer;
        private readonly Task _completedTask = Task.FromResult<object>(null);

        public LocalCacheToCacheAdapter(ILocalCache<TK, TV> cache, Func<TK, string> keySerializer)
        {
            _cache = cache;

            if (keySerializer != null)
            {
                _keySerializer = keySerializer;
            }
            else if (
                DefaultSettings.Cache.KeySerializers.TryGetSerializer<TK>(out var s) ||
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
            
            return _completedTask;
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

            return _completedTask;
        }

        private Key<TK> BuildKey(TK key)
        {
            return new Key<TK>(key, _keySerializer);
        }
    }
}