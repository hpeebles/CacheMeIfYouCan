using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class LocalCacheConfigurationManager : ILocalCacheFactory
    {
        private readonly ILocalCacheFactory _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        internal LocalCacheConfigurationManager(ILocalCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public LocalCacheConfigurationManager WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public LocalCacheConfigurationManager OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }

        public LocalCacheConfigurationManager OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }

        public LocalCacheConfigurationManager OnError(Action<CacheException> onError, bool append = false)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

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
    
    public class LocalCacheConfigurationManager<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        private readonly ILocalCacheFactory<TK, TV> _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;

        internal LocalCacheConfigurationManager(ILocalCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public LocalCacheConfigurationManager<TK, TV> WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public LocalCacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult<TK, TV>> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }
        
        public LocalCacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }

        public LocalCacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult<TK, TV>> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }
        
        public LocalCacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }

        public LocalCacheConfigurationManager<TK, TV> OnError(Action<CacheException> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }

        public LocalCacheConfigurationManager<TK, TV> OnError(Action<CacheException<TK>> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }
        
        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            var cache = _cacheFactory.Build(cacheName);

            return _notificationsEnabled
                ? new LocalCacheNotificationWrapper<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
        
        private LocalCacheConfigurationManager<TK, TV> OnGetResultImpl(Action<CacheGetResult<TK, TV>> onGetResult, bool append)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }
        
        private LocalCacheConfigurationManager<TK, TV> OnSetResultImpl(Action<CacheSetResult<TK, TV>> onSetResult, bool append)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }
        
        private LocalCacheConfigurationManager<TK, TV> OnErrorImpl(Action<CacheException<TK>> onError, bool append)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
    }
}
