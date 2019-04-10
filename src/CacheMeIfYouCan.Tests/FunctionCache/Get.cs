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
            
            Func<string, Task<string>> echo = new Echo(delay);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await cachedEcho("warmup");
            
            results.Clear();
            
            await cachedEcho("abc");
            
            results.Should().ContainSingle();
            results[0].Duration.Should().BeCloseTo(delay, TimeSpan.FromMilliseconds(200));
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

            var start = DateTime.UtcNow;

            await cachedEcho("abc");
            
            results.Should().ContainSingle();
            results[0].Start.Should().BeCloseTo(start, TimeSpan.FromMilliseconds(100));
        }
    }
}