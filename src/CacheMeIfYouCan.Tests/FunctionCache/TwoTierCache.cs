using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class TwoTierCache
    {
        [Fact]
        public async Task ChecksLocalFirstThenRemote()
        {
            var remoteCache = new TestCache<string, string>(x => x, x => x);
            var localCache1 = new TestLocalCache<string, string>();
            var localCache2 = new TestLocalCache<string, string>();
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);
            
            var results1 = new List<FunctionCacheGetResult>();
            var results2 = new List<FunctionCacheGetResult>();
            
            var cachedEcho1 = echo
                .Cached()
                .WithLocalCache(localCache1)
                .WithRemoteCache(remoteCache)
                .OnResult(results1.Add)
                .Build();
            
            var cachedEcho2 = echo
                .Cached()
                .WithLocalCache(localCache2)
                .WithRemoteCache(remoteCache)
                .OnResult(results2.Add)
                .Build();

            await cachedEcho1("123");
            Assert.Equal(1, results1.Count);
            Assert.Equal(Outcome.Fetch, results1[0].Outcome);

            await cachedEcho1("123");
            Assert.Equal(2, results1.Count);
            Assert.Equal(Outcome.FromCache, results1[1].Outcome);
            Assert.Equal(localCache1.CacheType, results1[1].CacheType);

            await cachedEcho2("123");
            Assert.Equal(1, results2.Count);
            Assert.Equal(Outcome.FromCache, results2[0].Outcome);
            Assert.Equal(remoteCache.CacheType, results2[0].CacheType);
            
            await cachedEcho2("123");
            Assert.Equal(2, results2.Count);
            Assert.Equal(Outcome.FromCache, results2[1].Outcome);
            Assert.Equal(localCache2.CacheType, results2[1].CacheType);
        }
    }
}