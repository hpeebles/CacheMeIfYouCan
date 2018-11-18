using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class Get
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task GetDurationIsAccurate(int seconds)
        {
            var delay = TimeSpan.FromSeconds(seconds);
            
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(new TestCache<string, string>(x => x, x => x, delay: delay))
                .OnResult(results.Add)
                .Build();
            
            await cachedEcho("abc");
            await cachedEcho("abc");
            
            Assert.Equal(2, results.Count);
            Assert.InRange(results[1].Duration, delay.Ticks * 0.99, delay.Ticks * 1.2);
        }
        
        [Fact]
        public async Task GetTimestampIsAccurate()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnResult(results.Add)
                .Build();
            
            var now = Timestamp.Now;

            await cachedEcho("abc");
            
            Assert.Single(results);
            Assert.True(now <= results[0].Start && results[0].Start < now + TimeSpan.FromMilliseconds(10).Ticks);
        }
    }
}