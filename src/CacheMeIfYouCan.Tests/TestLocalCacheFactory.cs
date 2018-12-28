using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Tests
{
    public class TestLocalCacheFactory : ILocalCacheFactory
    {
        private readonly TimeSpan? _delay;
        private readonly Func<bool> _error;

        public TestLocalCacheFactory(TimeSpan? delay = null, Func<bool> error = null)
        {
            _delay = delay;
            _error = error;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new TestLocalCache<TK, TV>(_delay, _error);
        }
    }
}
