using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
using FluentAssertions;
using Polly;
using Polly.CircuitBreaker;
using Xunit;

namespace CacheMeIfYouCan.Polly.Tests
{
    public class PollyTests
    {
        [Fact]
        public async Task DistributedCacheCircuitBreaker()
        {
            var policy = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
            
            var errorIndexes = new[] { 1, 3, 4 };

            var index = 0;
            
            var cache = new TestCacheFactory(error: () => errorIndexes.Contains(index++))
                .WithPolicy(policy)
                .Build<string, string>("test");
            
            var key = new Key<string>("123", "123");

            await cache.Set(key, "abc", TimeSpan.FromMinutes(1));

            Func<Task<GetFromCacheResult<string, string>>> func = () => cache.Get(key);
            await func.Should().ThrowAsync<CacheException>();
            
            (await cache.Get(key)).Should().Be("abc");

            await func.Should().ThrowAsync<CacheException>();
            await func.Should().ThrowAsync<CacheException>();

            (await func.Should().ThrowAsync<CacheException>())
                .Which.InnerException.Should().BeOfType<BrokenCircuitException>();

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            (await cache.Get(key)).Should().Be("abc");
        }
        
        [Fact]
        public void LocalCacheCircuitBreaker()
        {
            var policy = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromSeconds(1));
            
            var errorIndexes = new[] { 1, 3, 4 };

            var index = 0;
            
            var cache = new TestLocalCacheFactory(error: () => errorIndexes.Contains(index++))
                .WithPolicy(policy)
                .Build<string, string>("test");
            
            var key = new Key<string>("123", "123");

            cache.Set(key, "abc", TimeSpan.FromMinutes(1));

            Func<GetFromCacheResult<string, string>> func = () => cache.Get(key);
            func.Should().Throw<CacheException>();
            
            cache.Get(key).Should().Be("abc");

            func.Should().Throw<CacheException>();
            func.Should().Throw<CacheException>();
            
            func.Should()
                .Throw<CacheException>()
                .Which
                .InnerException
                .Should()
                .BeOfType<BrokenCircuitException>();

            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            
            cache.Get(key).Should().Be("abc");
        }
    }
}