using System;

namespace CacheMeIfYouCan.Tests
{
    public class TestCacheFactory : ICacheFactory
    {
        public ICache<T> Build<T>(MemoryCache<T> memoryCache = null, Func<T, string> serializer = null, Func<string, T> deserializer = null)
        {
            return new TestCache<T>(serializer, deserializer);
        }
    }
}
