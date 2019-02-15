using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class Get
    {
        private readonly CacheSetupLock _setupLock;

        public Get(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task GetDurationIsAccurate(int seconds)
        {
            var results = new List<FunctionCacheGetResult>();
            
            var delay = TimeSpan.FromSeconds(seconds);
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithDistributedCache(new TestCache<string, string>(x => x, x => x, delay: delay))
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("abc");
            await cachedEcho("abc");
            
            Assert.Equal(2, results.Count);
            Assert.InRange(results[1].Duration.Ticks, delay.Ticks * 0.99, delay.Ticks * 1.2);
        }
        
        [Fact]
        public async Task GetTimestampIsAccurate()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var now = Timestamp.Now;

            await cachedEcho("abc");
            
            results.Should().ContainSingle();
            Assert.True(now <= results[0].Start && results[0].Start < now + TimeSpan.FromMilliseconds(10).Ticks);
        }
    }
}