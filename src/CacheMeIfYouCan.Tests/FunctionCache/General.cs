using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class General
    {
        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task SubsequentCallsAreCached(string cacheType)
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithLocalCacheFactory(GetCacheFactory(cacheType))
                .OnFetch(fetches.Add)
                .Build();

            var first = true;
            for (var i = 0; i < 10; i++)
            {
                var timer = Stopwatch.StartNew();
                var result = await cachedEcho("test!");
                
                Assert.Equal("test!", result);
                if (first)
                {
                    Assert.True(timer.Elapsed > TimeSpan.FromMilliseconds(900));
                    first = false;
                }
                else
                {
                    Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(50));
                }
            }
            
            Assert.Single(fetches);
        }

        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task ShortTimeToLiveExpiresCorrectly(string cacheType)
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithTimeToLive(TimeSpan.FromMilliseconds(1))
                .WithLocalCacheFactory(GetCacheFactory(cacheType))
                .OnResult(results.Add)
                .Build();
            
            for (var i = 0; i < 10; i++)
            {
                await cachedEcho("abc");

                await Task.Delay(5);
            }
            
            Assert.Equal(10, results.Count);
            Assert.True(results.All(r => r.Results.Single().Outcome == Outcome.Fetch));
        }

        [Fact]
        public async Task TimeToLiveFactory()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithTimeToLiveFactory((k, v) => TimeSpan.FromMilliseconds(Int32.Parse(k)))
                .OnResult(results.Add)
                .Build();

            await cachedEcho("100");
            await cachedEcho("100");

            Assert.Equal(Outcome.FromCache, results.Last().Results.Last().Outcome);
            
            await cachedEcho("200");
            await cachedEcho("200");

            Assert.Equal(Outcome.FromCache, results.Last().Results.Last().Outcome);
            
            await Task.Delay(150);

            await cachedEcho("100");
            
            Assert.Equal(Outcome.Fetch, results.Last().Results.Last().Outcome);

            await cachedEcho("200");
            
            Assert.Equal(Outcome.FromCache, results.Last().Results.Last().Outcome);
           
            await Task.Delay(100);
            
            await cachedEcho("200");

            Assert.Equal(Outcome.Fetch, results.Last().Results.Last().Outcome);
        }

        private static ILocalCacheFactory GetCacheFactory(string cacheType)
        {
            switch (cacheType)
            {
                case "memory":
                    return new MemoryCacheFactory();
                
                case "dictionary":
                    return new DictionaryCacheFactory();
                
                default:
                    throw new Exception($"CacheType '{cacheType}' not recognised");
            }
        }
    }
}