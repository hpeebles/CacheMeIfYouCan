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
}