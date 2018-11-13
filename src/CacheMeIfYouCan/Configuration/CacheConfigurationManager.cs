using System;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Configuration
{
    public class CacheConfigurationManager : ICacheFactory
    {
        private readonly ICacheFactory _cacheFactory;
        private Action<CacheGetResult> _onGetResult;
        private Action<CacheSetResult> _onSetResult;

        internal CacheConfigurationManager(ICacheFactory cacheFactory)
        {
            _cacheFactory = cacheFactory;
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

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);
            
            if (_onGetResult != null || _onSetResult != null)
                cache = new CacheNotificationWrapper<TK, TV>(config.FunctionInfo, cache, _onGetResult, _onSetResult);

            return cache;
        }
    }
    
    public class CacheConfigurationManager<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly ICacheFactory<TK, TV> _cacheFactory;
        private Action<CacheGetResult<TK, TV>> _onGetResult;
        private Action<CacheSetResult<TK, TV>> _onSetResult;

        internal CacheConfigurationManager(ICacheFactory<TK, TV> cacheFactory)
        {
            _cacheFactory = cacheFactory;
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

        public bool RequiresStringKeys => _cacheFactory.RequiresStringKeys;
        
        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);
            
            if (_onGetResult != null || _onSetResult != null)
                cache = new CacheNotificationWrapper<TK, TV>(config.FunctionInfo, cache, _onGetResult, _onSetResult);

            return cache;
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
    }
}