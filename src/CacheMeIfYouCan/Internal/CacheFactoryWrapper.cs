using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    public class CacheFactoryWrapper : ICacheFactory
    {
        private readonly ICacheFactory _cacheFactory;
        private readonly ICacheWrapperFactory _cacheWrapper;

        public CacheFactoryWrapper(ICacheFactory cacheFactory, ICacheWrapperFactory cacheWrapper)
        {
            _cacheFactory = cacheFactory;
            _cacheWrapper = cacheWrapper;

            RequiresStringKeys = cacheFactory.RequiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }
        
        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _cacheWrapper.Wrap(cache);
        }
    }
    
    public class CacheFactoryWrapper<TK, TV> : ICacheFactory<TK, TV>
    {
        private readonly ICacheFactory<TK, TV> _cacheFactory;
        private readonly ICacheWrapperFactory<TK, TV> _cacheWrapper;

        public CacheFactoryWrapper(ICacheFactory<TK, TV> cacheFactory, ICacheWrapperFactory<TK, TV> cacheWrapper)
        {
            _cacheFactory = cacheFactory;
            _cacheWrapper = cacheWrapper;

            RequiresStringKeys = cacheFactory.RequiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }
        
        public ICache<TK, TV> Build(CacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _cacheWrapper.Wrap(cache);
        }
    }
}