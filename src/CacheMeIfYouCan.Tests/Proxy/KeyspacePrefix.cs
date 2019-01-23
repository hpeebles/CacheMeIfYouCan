using CacheMeIfYouCan.Configuration;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class KeyspacePrefix
    {
        private readonly CacheSetupLock _setupLock;

        public KeyspacePrefix(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public void AddedAsConstantSucceeds()
        {
            var cacheFactory = new KeyPrefixCheckingCacheFactory();
            
            ITest impl = new TestImpl();
            using (_setupLock.Enter())
            {
                impl
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory, "prefix")
                    .Build();
            }

            cacheFactory.BuildCount.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public void AddedViaFuncSucceeds()
        {
            var cacheFactory = new KeyPrefixCheckingCacheFactory();
            
            ITest impl = new TestImpl();
            using (_setupLock.Enter())
            {
                impl
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory, x => "prefix")
                    .Build();
            }

            cacheFactory.BuildCount.Should().BeGreaterThan(0);
        }
        
        private class KeyPrefixCheckingCacheFactory : IDistributedCacheFactory
        {
            public IDistributedCache<TK, TV> Build<TK, TV>(DistributedCacheConfig<TK, TV> config)
            {
                config.KeyspacePrefix.Should().Be("prefix");
                
                BuildCount++;
                
                return new TestCache<TK, TV>(null, null);
            }
            
            public int BuildCount { get; private set; }
        }
    }
}