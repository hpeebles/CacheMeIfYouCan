namespace CacheMeIfYouCan
{
    public interface ICacheFactory
    {
        ICache<T> Build<T>(CacheFactoryConfig<T> config);
    }
}