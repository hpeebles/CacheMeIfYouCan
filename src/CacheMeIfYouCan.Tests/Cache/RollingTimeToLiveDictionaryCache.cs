using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class RollingTimeToLiveDictionaryCache
    {
        [Fact]
        public async Task ExpiryIsPushedBackWhenKeyIsAccessed()
        {
            var cache = new RollingTimeToLiveDictionaryCache<string, string>("test", TimeSpan.FromMilliseconds(200));

            var keyString = Guid.NewGuid().ToString();
            var key = new Key<string>(keyString, keyString);
            
            cache.Set(key, keyString, TimeSpan.FromMinutes(1));

            for (var i = 0; i < 10; i++)
            {
                cache.Get(key).Success.Should().BeTrue();

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            cache.Get(key).Success.Should().BeFalse();
        }

        [Fact]
        public async Task KeyExpiresWhenOverallTimeToLiveHasPassed()
        {
            var cache = new RollingTimeToLiveDictionaryCache<string, string>("test", TimeSpan.FromMilliseconds(500));

            var keyString = Guid.NewGuid().ToString();
            var key = new Key<string>(keyString, keyString);
            
            cache.Set(key, keyString, TimeSpan.FromSeconds(1));

            var timer = Stopwatch.StartNew();
            
            while (timer.Elapsed < TimeSpan.FromMilliseconds(900))
            {
                cache.Get(key).Success.Should().BeTrue();

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            cache.Get(key).Success.Should().BeFalse();
        }
    }
}