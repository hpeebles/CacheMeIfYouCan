using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
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
            
            var errorIndexes = new[] { 0, 2, 3 };

            var index = 0;
            
            var cache = new TestCacheFactory(error: () => errorIndexes.Contains(index++))
                .WithPolicy(policy)
                .Build<string, string>("test");
            
            var key = new Key<string>("123", "123");

            await cache.Set(key, "abc", TimeSpan.FromMinutes(1));
            
            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            
            Assert.Equal("abc", await cache.Get(key));

            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            
            var exception = await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));

            Assert.IsType<BrokenCircuitException>(exception.InnerException);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            Assert.Equal("abc", await cache.Get(key));
        }
        
        [Fact]
        public void LocalCacheCircuitBreaker()
        {
            var policy = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromSeconds(1));
            
            var errorIndexes = new[] { 0, 2, 3 };

            var index = 0;
            
            var cache = new TestLocalCacheFactory(error: () => errorIndexes.Contains(index++))
                .WithPolicy(policy)
                .Build<string, string>("test");
            
            var key = new Key<string>("123", "123");

            cache.Set(key, "abc", TimeSpan.FromMinutes(1));
            
            Assert.ThrowsAny<CacheException>(() => cache.Get(key));
            
            Assert.Equal("abc", cache.Get(key));

            Assert.ThrowsAny<CacheException>(() => cache.Get(key));
            Assert.ThrowsAny<CacheException>(() => cache.Get(key));
            
            var exception = Assert.ThrowsAny<CacheException>(() => cache.Get(key));

            Assert.IsType<BrokenCircuitException>(exception.InnerException);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            
            Assert.Equal("abc", cache.Get(key));
        }
    }
}