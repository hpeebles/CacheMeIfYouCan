using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class General
    {
        [Fact]
        public async Task GetFromRedisCache()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));

            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c => c.ConnectionString = "redis-test")
                .OnResult(results.Add)
                .Build();

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            await cachedEcho(key);
            
            Assert.Equal(key, results[1].Results.Single().KeyString);
            Assert.Equal(Outcome.FromCache, results[1].Results.Single().Outcome);
            Assert.Equal("redis", results[1].Results.Single().CacheType);
        }
    }
}