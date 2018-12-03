using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class LocalCacheFactoryConfigurationManager : ILocalCacheFactory
    {
        private readonly ILocalCacheFactory _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        internal LocalCacheFactoryConfigurationManager(ILocalCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public LocalCacheFactoryConfigurationManager WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public LocalCacheFactoryConfigurationManager OnGetResult(
            Action<CacheGetResult> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, ordering);
            return this;
        }

        public LocalCacheFactoryConfigurationManager OnSetResult(
            Action<CacheSetResult> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, ordering);
            return this;
        }

        public LocalCacheFactoryConfigurationManager OnError(
            Action<CacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, ordering);
            return this;
        }
        
        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            var cache = _cacheFactory.Build<TK ,TV>(cacheName);
            
            return _notificationsEnabled
                ? new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
    }
    
    public class LocalCacheFactoryConfigurationManager<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory<TK, TV> _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;

        internal LocalCacheFactoryConfigurationManager(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public LocalCacheFactoryConfigurationManager<TK, TV> WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public LocalCacheFactoryConfigurationManager<TK, TV> OnGetResult(
            Action<CacheGetResult<TK, TV>> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onGetResult = ActionsHelper.Combine(_onGetResult, onGetResult, ordering);
            return this;
        }

        public LocalCacheFactoryConfigurationManager<TK, TV> OnSetResult(
            Action<CacheSetResult<TK, TV>> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onSetResult = ActionsHelper.Combine(_onSetResult, onSetResult, ordering);
            return this;
        }

        public LocalCacheFactoryConfigurationManager<TK, TV> OnError(
            Action<CacheException<TK>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            _onError = ActionsHelper.Combine(_onError, onError, ordering);
            return this;
        }
        
        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            var cache = _cacheFactory.Build(cacheName);

            return _notificationsEnabled
                ? new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
    }
}