namespace CacheMeIfYouCan
{
    public interface ICacheWrapperFactory
    {
        ICache<TK, TV> Wrap<TK, TV>(ICache<TK, TV> cache);
    }
    
    public interface ICacheWrapperFactory<TK, TV>
    {
        ICache<TK, TV> Wrap(ICache<TK, TV> cache);
    }
}