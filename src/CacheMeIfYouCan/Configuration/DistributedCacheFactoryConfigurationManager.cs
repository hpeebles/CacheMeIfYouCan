using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DistributedCacheFactoryConfigurationManager : IDistributedCacheFactory
    {
        private IDistributedCacheFactory _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        internal DistributedCacheFactoryConfigurationManager(IDistributedCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public DistributedCacheFactoryConfigurationManager WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public DistributedCacheFactoryConfigurationManager OnGetResult(
            Action<CacheGetResult> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onGetResult;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onGetResult = onGetResult;
            else if (ordering == ActionOrdering.Append)
                _onGetResult = x => { current(x); onGetResult(x); };
            else
                _onGetResult = x => { onGetResult(x); current(x); };

            return this;
        }

        public DistributedCacheFactoryConfigurationManager OnSetResult(
            Action<CacheSetResult> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onSetResult;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onSetResult = onSetResult;
            else if (ordering == ActionOrdering.Append)
                _onSetResult = x => { current(x); onSetResult(x); };
            else
                _onSetResult = x => { onSetResult(x); current(x); };

            return this;
        }

        public DistributedCacheFactoryConfigurationManager OnError(
            Action<CacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onError;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onError = onError;
            else if (ordering == ActionOrdering.Append)
                _onError = x => { current(x); onError(x); };
            else
                _onError = x => { onError(x); current(x); };

            return this;
        }
        
        public DistributedCacheFactoryConfigurationManager AddWrapper(IDistributedCacheWrapperFactory wrapper)
        {
            _cacheFactory = new DistributedCacheFactoryWrapper(_cacheFactory, wrapper);
            return this;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);
            
            return _notificationsEnabled
                ? new DistributedCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
    }
    
    public class DistributedCacheFactoryConfigurationManager<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private IDistributedCacheFactory<TK, TV> _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;

        internal DistributedCacheFactoryConfigurationManager(IDistributedCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
        }

        public DistributedCacheFactoryConfigurationManager<TK, TV> WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public DistributedCacheFactoryConfigurationManager<TK, TV> OnGetResult(
            Action<CacheGetResult<TK, TV>> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onGetResult;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onGetResult = onGetResult;
            else if (ordering == ActionOrdering.Append)
                _onGetResult = x => { current(x); onGetResult(x); };
            else
                _onGetResult = x => { onGetResult(x); current(x); };

            return this;
        }

        public DistributedCacheFactoryConfigurationManager<TK, TV> OnSetResult(
            Action<CacheSetResult<TK, TV>> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onSetResult;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onSetResult = onSetResult;
            else if (ordering == ActionOrdering.Append)
                _onSetResult = x => { current(x); onSetResult(x); };
            else
                _onSetResult = x => { onSetResult(x); current(x); };

            return this;
        }

        public DistributedCacheFactoryConfigurationManager<TK, TV> OnError(
            Action<CacheException<TK>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = _onError;
            if (current == null || ordering == ActionOrdering.Overwrite)
                _onError = onError;
            else if (ordering == ActionOrdering.Append)
                _onError = x => { current(x); onError(x); };
            else
                _onError = x => { onError(x); current(x); };

            return this;
        }

        public DistributedCacheFactoryConfigurationManager<TK, TV> AddWrapper(IDistributedCacheWrapperFactory<TK, TV> wrapper)
        {
            _cacheFactory = new DistributedCacheFactoryWrapper<TK, TV>(_cacheFactory, wrapper);
            return this;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public IDistributedCache<TK, TV> Build(DistributedCacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _notificationsEnabled
                ? new DistributedCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
    }
}