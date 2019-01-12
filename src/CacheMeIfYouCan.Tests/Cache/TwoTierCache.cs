using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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

            Assert.Single(distributedResults);
            Assert.Single(distributedResults.Single().Hits);
            Assert.Empty(distributedResults.Single().Misses);
            Assert.Single(localResults);
            Assert.Single(localResults.Single().Misses);
            Assert.Empty(localResults.Single().Hits);

            await cache.Get(key);

            Assert.Single(distributedResults);
            Assert.Equal(2, localResults.Count);
            Assert.Single(localResults.Last().Hits);
            Assert.Empty(localResults.Last().Misses);
        }
    }
}