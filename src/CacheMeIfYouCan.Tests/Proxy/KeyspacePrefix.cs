using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Tests.Helpers;
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
            var cacheFactory = new KeyspacePrefixCheckingCacheFactory();
            
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
            var cacheFactory = new KeyspacePrefixCheckingCacheFactory();
            
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
    }
}