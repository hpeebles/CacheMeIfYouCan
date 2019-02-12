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
        [InlineData(KeyEvents.None)]
        [InlineData(KeyEvents.Set)]
        [InlineData(KeyEvents.Del)]
        [InlineData(KeyEvents.All)]
        public async Task OnKeyChangedExternallyRemovesFromLocalCache(KeyEvents keyEvents)
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));

            var results = new List<FunctionCacheGetResult>();
            
            var localCache = new TestLocalCache<string, string>();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c =>
                {
                    c.ConnectionString = TestConnectionString.Value;
                    c.KeyEventsToSubscribeTo = keyEvents;
                })
                .WithLocalCache(localCache)
                .OnResult(results.Add)
                .Build();

            var redisClient = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            
            await cachedEcho(key1);
            await cachedEcho(key2);
            
            redisClient.GetDatabase().KeyExists(key1).Should().BeTrue();
            redisClient.GetDatabase().KeyExists(key2).Should().BeTrue();
            
            localCache.Values.ContainsKey(key1).Should().BeTrue();
            localCache.Values.ContainsKey(key2).Should().BeTrue();

            redisClient.GetDatabase().StringSet(key1, "123", TimeSpan.FromMinutes(1));
            redisClient.GetDatabase().KeyDelete(key2);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            localCache.Values.ContainsKey(key1).Should().Be(!keyEvents.HasFlag(KeyEvents.Set));
            localCache.Values.ContainsKey(key2).Should().Be(!keyEvents.HasFlag(KeyEvents.Del));
        }
    }
}