using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
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
            
            Func<string, Task<string>> echo = new Echo();
            using (_setupLock.Enter())
            {
                echo
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory, "prefix")
                    .Build();
            }

            cacheFactory.BuildCount.Should().Be(1);
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