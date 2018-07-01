namespace CacheMeIfYouCan.Tests
{
    public class TestCacheFactory : ICacheFactory
    {
        public ICache<T> Build<T>(CacheFactoryConfig<T> config)
        {
            return new TestCache<T>(config.Serializer, config.Deserializer);
        }
    }
}
