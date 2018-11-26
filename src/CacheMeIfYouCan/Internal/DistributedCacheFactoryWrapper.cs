using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Internal
{
    public class DistributedCacheFactoryWrapper : IDistributedCacheFactory
    {
        private readonly IDistributedCacheFactory _cacheFactory;
        private readonly IDistributedCacheWrapperFactory _cacheWrapper;

        public DistributedCacheFactoryWrapper(IDistributedCacheFactory cacheFactory, IDistributedCacheWrapperFactory cacheWrapper)
        {
            _cacheFactory = cacheFactory;
            _cacheWrapper = cacheWrapper;

            RequiresStringKeys = cacheFactory.RequiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }
        
        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _cacheWrapper.Wrap(cache);
        }
    }
    
    public class DistributedCacheFactoryWrapper<TK, TV> : IDistributedCacheFactory<TK, TV>
    {
        private readonly IDistributedCacheFactory<TK, TV> _cacheFactory;
        private readonly IDistributedCacheWrapperFactory<TK, TV> _cacheWrapper;

        public DistributedCacheFactoryWrapper(IDistributedCacheFactory<TK, TV> cacheFactory, IDistributedCacheWrapperFactory<TK, TV> cacheWrapper)
        {
            _cacheFactory = cacheFactory;
            _cacheWrapper = cacheWrapper;

            RequiresStringKeys = cacheFactory.RequiresStringKeys;
        }
        
        public bool RequiresStringKeys { get; }
        
        public IDistributedCache<TK, TV> Build(DistributedCacheFactoryConfig<TK, TV> config)
        {
            var cache = _cacheFactory.Build(config);

            return _cacheWrapper.Wrap(cache);
        }
    }
}