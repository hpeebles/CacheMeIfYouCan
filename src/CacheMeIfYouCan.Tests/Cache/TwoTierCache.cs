using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class TwoTierCache
    {
        private readonly CacheSetupLock _setupLock;

        public TwoTierCache(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task ChecksLocalThenDistributed()
        {
            var distributedResults = new List<CacheGetResult>();
            var localResults = new List<CacheGetResult>();

            var key = Guid.NewGuid().ToString();
            
            ICache<string, string> cache;
            using (_setupLock.Enter())
            {
                var distributed = new TestCacheFactory()
                    .OnGetResult(distributedResults.Add)
                    .Build<string, string>("distributed");

                await distributed.Set(new Key<string>(key, key), key, TimeSpan.FromMinutes(1));
                
                var local = new TestLocalCacheFactory()
                    .OnGetResult(localResults.Add)
                    .Build<string, string>("local");

                cache = new TwoTierCache<string, string>(distributed, local);
            }

            await cache.Get(key);

            distributedResults.Should().ContainSingle();
            distributedResults.Single().Hits.Should().ContainSingle();
            Assert.Empty(distributedResults.Single().Misses);
            localResults.Should().ContainSingle();
            localResults.Single().Misses.Should().ContainSingle();
            Assert.Empty(localResults.Single().Hits);

            await cache.Get(key);

            distributedResults.Should().ContainSingle();
            Assert.Equal(2, localResults.Count);
            localResults.Last().Hits.Should().ContainSingle();
            Assert.Empty(localResults.Last().Misses);
        }
    }
}