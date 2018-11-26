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
        
        public bool RequiresStringKeys => true;
        
        public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheFactoryConfig<TK, TV> config)
        {
            return new TestCache<TK, TV>(config.ValueSerializer, config.ValueDeserializer, null, _delay, _error);
        }
    }
}
