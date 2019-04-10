using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    public class ResetConnection
    {
        [Fact]
        public async Task ResetConnectionSucceeds()
        {
            Func<string, Task<string>> echo = new Echo();

            var connection = new RedisConnection(TestConnectionString.Value);

            connection.Connect();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c => c.Connection = connection)
                .Build();

            var key = Guid.NewGuid().ToString();

            Func<Task> func = async () => await cachedEcho(key);

            await func.Should().NotThrowAsync();
            
            connection.Get().Dispose();

            await func.Should().ThrowAsync<Exception>();

            connection.Reset(false).Should().BeTrue();

            await func.Should().NotThrowAsync();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SubscriptionsAreRestoredAfterReset(bool subscribeToKeyChanges)
        {
            Func<string, Task<string>> echo = new Echo();

            var connection = new RedisConnection(TestConnectionString.Value);

            connection.Connect();
            
            var results = new List<FunctionCacheGetResult>();
            
            var localCache = new TestLocalCache<string, string>();
            
            var cachedEcho = echo
                .Cached()
                .WithRedis(c =>
                {
                    c.Connection = connection;
                    c.KeyEventsToSubscribeTo = subscribeToKeyChanges ? KeyEvents.Del : KeyEvents.None;
                })
                .WithLocalCache(localCache)
                .OnResult(results.Add)
                .Build();

            var redisClient = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            await RunSubscriberTest();

            connection.Reset(false);

            await RunSubscriberTest();
            
            async Task RunSubscriberTest()
            {
                var key = Guid.NewGuid().ToString();

                await cachedEcho(key);

                redisClient.GetDatabase().KeyExists(key).Should().BeTrue();

                localCache.Values.ContainsKey(key).Should().BeTrue();

                redisClient.GetDatabase().KeyDelete(key);

                await Task.Delay(TimeSpan.FromSeconds(1));

                localCache.Values.ContainsKey(key).Should().Be(!subscribeToKeyChanges);
            }
        }
    }
}