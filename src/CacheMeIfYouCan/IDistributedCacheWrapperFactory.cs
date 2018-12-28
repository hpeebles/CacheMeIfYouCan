namespace CacheMeIfYouCan
{
    public interface IDistributedCacheWrapperFactory
    {
        IDistributedCache<TK, TV> Wrap<TK, TV>(IDistributedCache<TK, TV> cache);
    }
    
    public interface IDistributedCacheWrapperFactory<TK, TV>
    {
        IDistributedCache<TK, TV> Wrap(IDistributedCache<TK, TV> cache);
    }
    
    internal class DistributedCacheWrapperFactoryToGenericAdapter<TK, TV> : IDistributedCacheWrapperFactory<TK, TV>
    {
        private readonly IDistributedCacheWrapperFactory _wrapperFactory;

        public DistributedCacheWrapperFactoryToGenericAdapter(IDistributedCacheWrapperFactory wrapperFactory)
        {
            _wrapperFactory = wrapperFactory;
        }

        public IDistributedCache<TK, TV> Wrap(IDistributedCache<TK, TV> cache)
        {
            return _wrapperFactory.Wrap(cache);
        }
    }
}