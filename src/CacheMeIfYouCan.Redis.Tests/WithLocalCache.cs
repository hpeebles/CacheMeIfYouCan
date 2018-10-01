using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
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
                    c.ConnectionString = "redis-test";
                    c.KeySpacePrefix = "CacheMeIfYouCanTest";
                })
                .WithLocalCache(localCache)
                .OnResult(results.Add)
                .Build();

            var redisClient = ConnectionMultiplexer.Connect("redis-test");

            var key = Guid.NewGuid().ToString();
            var redisKey = "CacheMeIfYouCanTest" + key;
            
            await cachedEcho(key);
            
            Assert.True(redisClient.GetDatabase().KeyExists(redisKey));
            Assert.True(localCache.Values.ContainsKey(key));

            if (action == "set")
                redisClient.GetDatabase().StringSet(redisKey, "123");
            else
                redisClient.GetDatabase().KeyDelete(redisKey);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            Assert.False(localCache.Values.ContainsKey(key));
        }
    }
}