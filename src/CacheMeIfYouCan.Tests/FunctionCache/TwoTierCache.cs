using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class TwoTierCache
    {
        private readonly CacheSetupLock _setupLock;

        public TwoTierCache(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task ChecksLocalFirstThenDistributed()
        {
            var distributedCache = new TestCache<string, string>(x => x, x => x);
            var localCache1 = new TestLocalCache<string, string>();
            var localCache2 = new TestLocalCache<string, string>();
            
            var results1 = new List<FunctionCacheGetResult>();
            var results2 = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho1;
            Func<string, Task<string>> cachedEcho2;
            using (_setupLock.Enter())
            {
                cachedEcho1 = echo
                    .Cached()
                    .WithLocalCache(localCache1)
                    .WithDistributedCache(distributedCache)
                    .OnResult(results1.Add)
                    .Build();

                cachedEcho2 = echo
                    .Cached()
                    .WithLocalCache(localCache2)
                    .WithDistributedCache(distributedCache)
                    .OnResult(results2.Add)
                    .Build();
            }

            await cachedEcho1("123");
            results1.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results1[0].Results.Single().Outcome);

            await cachedEcho1("123");
            Assert.Equal(2, results1.Count);
            Assert.Equal(Outcome.FromCache, results1[1].Results.Single().Outcome);
            Assert.Equal(localCache1.CacheType, results1[1].Results.Single().CacheType);

            await cachedEcho2("123");
            results2.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results2[0].Results.Single().Outcome);
            Assert.Equal(distributedCache.CacheType, results2[0].Results.Single().CacheType);
            
            await cachedEcho2("123");
            Assert.Equal(2, results2.Count);
            Assert.Equal(Outcome.FromCache, results2[1].Results.Single().Outcome);
            Assert.Equal(localCache2.CacheType, results2[1].Results.Single().CacheType);
        }

        [Fact]
        public async Task KeyRemovedFromLocalIfDistributedNotifiesItAsChanged()
        {
            var localCache = new TestLocalCache<string, string>();
            var distributedCache = new TestCache<string, string>(x => x, x => x);

            var results = new List<FunctionCacheGetResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(localCache)
                    .WithDistributedCache(distributedCache)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("123");
            results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results[0].Results.Single().Outcome);

            await cachedEcho("123");
            Assert.Equal(2, results.Count);
            Assert.Equal(Outcome.FromCache, results[1].Results.Single().Outcome);
            Assert.Equal(localCache.CacheType, results[1].Results.Single().CacheType);

            distributedCache.NotifyChanged(new Key<string>("123", "123"));

            await cachedEcho("123");
            Assert.Equal(3, results.Count);
            Assert.Equal(Outcome.Fetch, results[2].Results.Single().Outcome);
        }

        [Fact]
        public async Task LocalCacheTimeToLiveOverride()
        {
            var localCache = new TestLocalCache<string, string>();
            var distributedCache = new TestCache<string, string>(x => x, x => x);

            var results = new List<FunctionCacheGetResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(localCache)
                    .WithDistributedCache(distributedCache)
                    .WithLocalCacheTimeToLiveOverride(TimeSpan.FromMilliseconds(500))
                    .OnResult(results.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await cachedEcho(key);
            await cachedEcho(key);

            results.Last().Results.Single().CacheType.Should().Be("test-local");

            await Task.Delay(TimeSpan.FromSeconds(1));

            await cachedEcho(key);

            results.Last().Results.Single().CacheType.Should().Be("test");
        }
    }
}