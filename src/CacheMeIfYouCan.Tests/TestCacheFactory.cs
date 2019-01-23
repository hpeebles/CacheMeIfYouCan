using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Tests
{
    public class TestCacheFactory : IDistributedCacheFactory
    {
        private readonly TimeSpan? _delay;
        private readonly Func<bool> _error;

        public TestCacheFactory(TimeSpan? delay = null, Func<bool> error = null)
        {
            _delay = delay;
            _error = error;
        }
        
        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config)
        {
            return new TestCache<TK, TV>(config.ValueSerializer, config.ValueDeserializer, _delay, _error, config.CacheName);
        }
    }
}
