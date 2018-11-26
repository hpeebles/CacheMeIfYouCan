using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class DistributedCacheConfigurationManager : IDistributedCacheFactory
    {
        private IDistributedCacheFactory _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;
        private Action<CacheException> _onError;

        internal DistributedCacheConfigurationManager(IDistributedCacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        public DistributedCacheConfigurationManager WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public DistributedCacheConfigurationManager OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }

        public DistributedCacheConfigurationManager OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }

        public DistributedCacheConfigurationManager OnError(Action<CacheException> onError, bool append = false)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
        
        public DistributedCacheConfigurationManager AddWrapper(IDistributedCacheWrapperFactory wrapper)
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
    
    public class DistributedCacheConfigurationManager<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private IDistributedCacheFactory<TK, TV> _cacheFactory;
        private bool _notificationsEnabled = true;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;
        private Action<CacheException<TK>> _onError;

        internal DistributedCacheConfigurationManager(IDistributedCacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory ?? throw new ArgumentNullException(nameof(cacheFactory));
        }

        public DistributedCacheConfigurationManager<TK, TV> WithNotificationsEnabled(bool enabled)
        {
            _notificationsEnabled = enabled;
            return this;
        }
        
        public DistributedCacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult<TK, TV>> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }
        
        public DistributedCacheConfigurationManager<TK, TV> OnGetResult(Action<CacheGetResult> onGetResult, bool append = false)
        {
            return OnGetResultImpl(onGetResult, append);
        }

        public DistributedCacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult<TK, TV>> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }
        
        public DistributedCacheConfigurationManager<TK, TV> OnSetResult(Action<CacheSetResult> onSetResult, bool append = false)
        {
            return OnSetResultImpl(onSetResult, append);
        }

        public DistributedCacheConfigurationManager<TK, TV> OnError(Action<CacheException> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }

        public DistributedCacheConfigurationManager<TK, TV> OnError(Action<CacheException<TK>> onError, bool append = false)
        {
            return OnErrorImpl(onError, append);
        }

        public DistributedCacheConfigurationManager<TK, TV> AddWrapper(IDistributedCacheWrapperFactory<TK, TV> wrapper)
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
        
        private DistributedCacheConfigurationManager<TK, TV> OnGetResultImpl(Action<CacheGetResult<TK, TV>> onGetResult, bool append)
        {
            if (onGetResult == null || !append)
                _onGetResult = onGetResult;
            else
                _onGetResult = x => { _onGetResult(x); onGetResult(x); };

            return this;
        }
        
        private DistributedCacheConfigurationManager<TK, TV> OnSetResultImpl(Action<CacheSetResult<TK, TV>> onSetResult, bool append)
        {
            if (onSetResult == null || !append)
                _onSetResult = onSetResult;
            else
                _onSetResult = x => { _onSetResult(x); onSetResult(x); };

            return this;
        }
        
        private DistributedCacheConfigurationManager<TK, TV> OnErrorImpl(Action<CacheException<TK>> onError, bool append)
        {
            if (onError == null || !append)
                _onError = onError;
            else
                _onError = x => { _onError(x); onError(x); };

            return this;
        }
    }
}