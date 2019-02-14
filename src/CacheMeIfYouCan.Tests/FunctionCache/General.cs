using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class General
    {
        private readonly CacheSetupLock _setupLock;

        public General(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task SubsequentCallsAreCached_Async(string cacheType)
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCacheFactory(GetCacheFactory(cacheType))
                    .OnFetch(fetches.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();
            
            var first = true;
            for (var i = 0; i < 10; i++)
            {
                var timer = Stopwatch.StartNew();
                var result = await cachedEcho(key);
                
                result.Should().Be(key);
                if (first)
                {
                    timer.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(900));
                    first = false;
                }
                else
                {
                    timer.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
                }
            }
            
            fetches.Should().ContainSingle();
        }

        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public void SubsequentCallsAreCached_Sync(string cacheType)
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, string> echo = new EchoSync(TimeSpan.FromSeconds(1));
            Func<string, string> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCacheFactory(GetCacheFactory(cacheType))
                    .OnFetch(fetches.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            var first = true;
            for (var i = 0; i < 10; i++)
            {
                var timer = Stopwatch.StartNew();
                var result = cachedEcho(key);
                
                result.Should().Be(key);
                if (first)
                {
                    timer.Elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(900));
                    first = false;
                }
                else
                {
                    timer.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
                }
            }
            
            fetches.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task ShortTimeToLiveExpiresCorrectly(string cacheType)
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMilliseconds(1))
                    .WithLocalCacheFactory(GetCacheFactory(cacheType))
                    .OnResult(results.Add)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            for (var i = 0; i < 10; i++)
            {
                await cachedEcho(key);

                await Task.Delay(5);
            }
            
            results.Should().HaveCount(10);
            results.ForEach(r => r.Results.Single().Outcome.Should().Be(Outcome.Fetch));
        }

        [Fact]
        public async Task TimeToLiveFactory()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLiveFactory((k, v) => TimeSpan.FromMilliseconds(Int32.Parse(k)))
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("100");
            await cachedEcho("100");

            results.Last().Results.Last().Outcome.Should().Be(Outcome.FromCache);
            
            await cachedEcho("200");
            await cachedEcho("200");

            results.Last().Results.Last().Outcome.Should().Be(Outcome.FromCache);
            
            await Task.Delay(150);

            await cachedEcho("100");
            
            results.Last().Results.Last().Outcome.Should().Be(Outcome.Fetch);

            await cachedEcho("200");
            
            results.Last().Results.Last().Outcome.Should().Be(Outcome.FromCache);
           
            await Task.Delay(100);
            
            await cachedEcho("200");

            results.Last().Results.Last().Outcome.Should().Be(Outcome.Fetch);
        }

        [Fact]
        public async Task Named()
        {
            var results = new List<FunctionCacheGetResult>();

            var name = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .Named(name)
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("123");

            results.Single().FunctionName.Should().Be(name);
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