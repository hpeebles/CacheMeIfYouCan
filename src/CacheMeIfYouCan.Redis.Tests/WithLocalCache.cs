using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class WithLocalCache
    {
        [Theory]
        [InlineData("set")]
        [InlineData("delete")]
        public async Task OnKeyChangedExternallyRemovesFromLocalCache(string action)
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));

            var results = new List<FunctionCacheGetResult>();
            
            var localCache = new TestLocalCache<string, string>();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c =>
                {
                    c.ConnectionString = TestConnectionString.Value;
                })
                .WithLocalCache(localCache)
                .OnResult(results.Add)
                .Build();

            var redisClient = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            var key = Guid.NewGuid().ToString();
            var redisKey = "CacheMeIfYouCanTest" + key;
            
            await cachedEcho(key);
            
            redisClient.GetDatabase().KeyExists(redisKey).Should().BeTrue();
            localCache.Values.ContainsKey(key).Should().BeTrue();

            if (action == "set")
                redisClient.GetDatabase().StringSet(redisKey, "123");
            else
                redisClient.GetDatabase().KeyDelete(redisKey);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            localCache.Values.ContainsKey(key).Should().BeFalse();
        }
    }
}