using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Tests.Common;
using CacheMeIfYouCan.Tests.Helpers;
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
            var cacheFactory = new KeyspacePrefixCheckingCacheFactory();
            
            Func<string, Task<string>> echo = new Echo();
            using (_setupLock.Enter())
            {
                echo
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory)
                    .WithKeyspacePrefix("prefix")
                    .Build();
            }

            cacheFactory.BuildCount.Should().Be(1);
        }
    }
}