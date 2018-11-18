using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CacheConfigurationManager : ICacheFactory
    {
        private ICacheFactory _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        internal CacheConfigurationManager(ICacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public CacheConfigurationManager WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public CacheConfigurationManager OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }

        public CacheConfigurationManager OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }

        public CacheConfigurationManager OnError(Action<CacheException> onError, bool append = false)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
        
        public CacheConfigurationManager AddWrapper(ICacheWrapperFactory wrapper)
        {
            _cacheFactory = new CacheFactoryWrapper(_cacheFactory, wrapper);
            return this;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);
            
            return _notificationsEnabled
                ? new CacheNotificationWrapperInternal<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
    }
    
    public class CacheConfigurationManager<TK, TV> : ICacheFactory<TK, TV>
    {
        private ICacheFactory<TK, TV> _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;

        internal CacheConfigurationManager(ICacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
        }

        public CacheConfigurationManager<TK, TV> WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public CacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult<TK, TV>> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }
        
        public CacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }

        public CacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult<TK, TV>> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }
        
        public CacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }

        public CacheConfigurationManager<TK, TV> OnError(Action<CacheException> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }

        public CacheConfigurationManager<TK, TV> OnError(Action<CacheException<TK>> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }

        public CacheConfigurationManager<TK, TV> AddWrapper(ICacheWrapperFactory<TK, TV> wrapper)
        {
            _cacheFactory = new CacheFactoryWrapper<TK, TV>(_cacheFactory, wrapper);
            return this;
        }

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _notificationsEnabled
                ? new CacheNotificationWrapperInternal<TK, TV>(cache, _onGetResult, _onSetResult, _onError)
                : cache;
        }
        
        private CacheConfigurationManager<TK, TV> OnGetResultImpl(Action<CacheGetResult<TK, TV>> onGetResult, bool append)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }
        
        private CacheConfigurationManager<TK, TV> OnSetResultImpl(Action<CacheSetResult<TK, TV>> onSetResult, bool append)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }
        
        private CacheConfigurationManager<TK, TV> OnErrorImpl(Action<CacheException<TK>> onError, bool append)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
    }
}