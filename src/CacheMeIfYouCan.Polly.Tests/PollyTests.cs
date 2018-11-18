using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
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
        public async Task CircuitBreaker()
        {
            var policy = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(1));
            
            var errorIndexes = new[] { 0, 2, 3 };

            var index = 0;
            
            var cache = new TestCacheFactory(error: () => errorIndexes.Contains(index++))
                .Configure(c => c.WithPolicy(policy))
                .Build(new CacheFactoryConfig<string, string>());
            
            var key = new Key<string>("123", "123");

            await cache.Set(key, "abc", TimeSpan.FromMinutes(1));
            
            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            
            Assert.Equal("abc", await cache.Get(key));

            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            
            var exception = await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));

            Assert.True(exception.InnerException is BrokenCircuitException);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            Assert.Equal("abc", await cache.Get(key));
        }
    }
}