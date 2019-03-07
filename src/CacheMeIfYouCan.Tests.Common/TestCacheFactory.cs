﻿using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Tests.Common
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
        
        public IDistributedCache<TK, TV> Build<TK, TV>(IDistributedCacheConfig<TK, TV> config)
        {
            return new TestCache<TK, TV>(
                config.ValueSerializer,
                config.ValueDeserializer,
                config.ValueByteSerializer,
                config.ValueByteDeserializer,
                _delay,
                _error,
                config.CacheName);
        }
    }
}
