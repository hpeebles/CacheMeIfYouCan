using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
using FluentAssertions;
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
                .WithRedis(c => c.ConnectionString = TestConnectionString.Value)
                .OnResult(results.Add)
                .Build();

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            await cachedEcho(key);
            
            results[1].Results.Single().KeyString.Should().Be(key);
            results[1].Results.Single().Outcome.Should().Be(Outcome.FromCache);
            results[1].Results.Single().CacheType.Should().Be("redis");
        }
    }
}