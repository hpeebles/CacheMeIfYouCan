namespace CacheMeIfYouCan
{
    public interface ILocalCacheWrapperFactory
    {
        ILocalCache<TK, TV> Wrap<TK, TV>(ILocalCache<TK, TV> cache);
    }
    
    public interface ILocalCacheWrapperFactory<TK, TV>
    {
        ILocalCache<TK, TV> Wrap(ILocalCache<TK, TV> cache);
    }
    
    internal class LocalCacheWrapperFactoryToGenericAdaptor<TK, TV> : ILocalCacheWrapperFactory<TK, TV>
    {
        private readonly ILocalCacheWrapperFactory _wrapperFactory;

        public LocalCacheWrapperFactoryToGenericAdaptor(ILocalCacheWrapperFactory wrapperFactory)
        {
            _wrapperFactory = wrapperFactory;
        }

        public ILocalCache<TK, TV> Wrap(ILocalCache<TK, TV> cache)
        {
            return _wrapperFactory.Wrap(cache);
        }
    }
}