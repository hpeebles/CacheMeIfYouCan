using System;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Tests
{
    public class TestCacheFactory : ICacheFactory
    {
        public bool RequiresStringKeys => true;
        
        public ICache<TK, TV> Build<TK, TV>(CacheFactoryConfig<TK, TV> config)
        {
            return new TestCache<TK, TV>(config.ValueSerializer, config.ValueDeserializer);
        }
    }
}
