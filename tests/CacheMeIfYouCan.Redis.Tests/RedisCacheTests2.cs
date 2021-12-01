using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Serializers.ProtoBuf;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;

namespace CacheMeIfYouCan.Redis.Tests
{
    /// <summary>
    /// Tests for <see cref="RedisCache{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class RedisCacheTests2
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully(bool useSerializer)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, useSerializer: useSerializer);

            var tasks = Enumerable
                .Range(1, 5)
                .Select(i => Task.Run(async () =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var keys = Enumerable.Range((1000 * i) + j, i).ToArray();
                        await cache.SetMany(1, keys.Select(k => new KeyValuePair<int, TestClass>(k, new TestClass(k))).ToArray(), TimeSpan.FromSeconds(1));
                        var values = await cache.GetMany(1, keys);
                        values.Select(kv => kv.Key).Should().BeEquivalentTo(keys);
                        foreach (var (key, value) in values)
                        {
                            value.Value.IntValue.Should().Be(key);
                            value.Value.StringValue.Should().Be(key.ToString());
                            value.TimeToLive.Should().BePositive().And.BeLessThan(TimeSpan.FromSeconds(1));
                        }
                        
                        Thread.Yield();
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }
        
        [Fact]
        public async Task GetMany_WithSomeKeysNotSet_ReturnsAllKeysWhichHaveValues()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);

            var keys = Enumerable.Range(1, 10).ToArray();
            var values = keys.Where(k => k % 2 == 0).Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i))).ToArray();

            await cache.SetMany(1, values, TimeSpan.FromSeconds(1));

            var fromCache = await cache.GetMany(1, keys);

            fromCache.Select(kv => kv.Key).Should().BeEquivalentTo(values.Select(kv => kv.Key));
        }
        
        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task WithTimeToLive_DataExpiredCorrectly(int timeToLiveMs)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            await cache.SetMany(1, new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromMilliseconds(timeToLiveMs));
            (await cache.GetMany(1, new[] { 1 })).Should().ContainSingle();
            
            Thread.Sleep(timeToLiveMs + 20);

            (await cache.GetMany(1, new[] { 1 })).Should().BeEmpty();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhenFireAndForgetEnabled_SetOperationsCompleteImmediately(bool useFireAndForget)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, useFireAndForget);

            var task = cache.SetMany(1, new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromSeconds(1));

            task.IsCompleted.Should().Be(useFireAndForget);
        }
        
        [Fact]
        public async Task WhenConnectionDisposed_ThrowsObjectDisposedException()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            connection.Dispose();

            Func<Task> task = () => cache.GetMany(1, new[] { 1 });

            await task.Should().ThrowExactlyAsync<ObjectDisposedException>().WithMessage($"* '{connection.GetType()}'.");
        }
        
        [Theory]
        [InlineData("x")]
        [InlineData("000")]
        public async Task Nulls_StoredAsChosenNullValue(string nullValue)
        {
            var keyPrefix = Guid.NewGuid().ToString();

            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, keyPrefix: keyPrefix, nullValue: nullValue);

            using var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value);
            var redisDb = connectionMultiplexer.GetDatabase();
            
            await cache.SetMany(1, new[] { new KeyValuePair<int, TestClass>(1, null) }, TimeSpan.FromSeconds(1));
            var values = await cache.GetMany(1, new[] { 1 });
            values.Should().ContainSingle();
            var (_, value) = values.Single();
            value.Value.Should().BeNull();
            value.TimeToLive.Should().BePositive().And.BeLessThan(TimeSpan.FromSeconds(1));

            var rawValueInRedis = await redisDb.StringGetAsync(keyPrefix + "1_1");
            rawValueInRedis.Should().Be(nullValue);
        }
        
        [Theory]
        [InlineData(KeyEventType.None)]
        [InlineData(KeyEventType.Set)]
        [InlineData(KeyEventType.Del)]
        [InlineData(KeyEventType.All)]
        public async Task SubscriberEnabled_OnKeyChangedExternally_NotificationRaisedAsExpected(KeyEventType eventTypes)
        {
            var keyPrefix = Guid.NewGuid().ToString();
            
            using var connection = BuildConnection();
            using var cache1 = BuildRedisCache(connection, keyPrefix: keyPrefix, keyEventTypesToSubscribeTo: eventTypes);
            using var cache2 = BuildRedisCache(connection, keyPrefix: keyPrefix);

            var values = Enumerable
                .Range(0, 10)
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();

            var expectedEvents = new List<(string, string, KeyEventType)>();
            
            if (eventTypes.HasFlag(KeyEventType.Set))
                expectedEvents.AddRange(values.Select(kv => ("1", kv.Key.ToString(), KeyEventType.Set)));
            
            if (eventTypes.HasFlag(KeyEventType.Del))
                expectedEvents.Add(("1", "1", KeyEventType.Del));
            
            if (eventTypes.HasFlag(KeyEventType.Expired))
                expectedEvents.AddRange(values.Skip(2).Select(kv => ("1", kv.Key.ToString(), KeyEventType.Expired)));

            expectedEvents = expectedEvents
                .OrderBy(t => t.Item1)
                .ThenBy(t => t.Item2)
                .ToList();
            
            using var countdown = new CountdownEvent(expectedEvents.Count);
            var keysChangedRemotely = new ConcurrentBag<(string, string, KeyEventType)>();

            using var _ = cache1.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely.Add((t.OuterKey, t.InnerKey, t.EventType));
                countdown.Signal();
            });
            
            await cache1
                .SetMany(1, values, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            keysChangedRemotely.Should().BeEmpty();
            
            await cache2
                .SetMany(1, values.ToArray(), TimeSpan.FromMilliseconds(500))
                .ConfigureAwait(false);

            await cache1.TryRemove(1, 0).ConfigureAwait(false);
            await cache2.TryRemove(1, 1).ConfigureAwait(false);

            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            keysChangedRemotely.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Should().BeEquivalentTo(expectedEvents);
        }
        
        [Fact]
        public async Task MultipleSubscriptionsUsingSingleConnection_NotificationsAreSentToCorrectSubscriber()
        {
            var keyPrefix1 = Guid.NewGuid().ToString();
            var keyPrefix2 = Guid.NewGuid().ToString();
            
            using var connection = BuildConnection();
            using var cache1 = BuildRedisCache(connection, keyPrefix: keyPrefix1, keyEventTypesToSubscribeTo: KeyEventType.Set);
            using var cache2 = BuildRedisCache(connection, keyPrefix: keyPrefix2, keyEventTypesToSubscribeTo: KeyEventType.Set);
            using var cache3 = BuildRedisCache(connection, keyPrefix: keyPrefix1);
            using var cache4 = BuildRedisCache(connection, keyPrefix: keyPrefix2);

            using var countdown = new CountdownEvent(20); 
            
            var keysChangedRemotely1 = new ConcurrentBag<(string, string, KeyEventType)>(); 
            var keysChangedRemotely2 = new ConcurrentBag<(string, string, KeyEventType)>(); 

            using var _ = cache1.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely1.Add((t.OuterKey, t.InnerKey, t.EventType));
                countdown.Signal();
            });
            
            using var __ = cache2.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely2.Add((t.OuterKey, t.InnerKey, t.EventType));
                countdown.Signal();
            });
            
            var values1 = Enumerable
                .Range(0, 10)
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();

            var values2 = Enumerable
                .Range(10, 10)
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();
            
            await cache3
                .SetMany(1, values1, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            await cache4
                .SetMany(2, values2, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            var expectedEvents1 = values1.Select(kv => ("1", kv.Key.ToString(), KeyEventType.Set)).ToArray();
            var expectedEvents2 = values2.Select(kv => ("2", kv.Key.ToString(), KeyEventType.Set)).ToArray();
            
            keysChangedRemotely1.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Should().BeEquivalentTo(expectedEvents1);
            keysChangedRemotely2.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Should().BeEquivalentTo(expectedEvents2);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1000, false)]
        public async Task TestTelemetryOnCommand(int threshold, bool anyTrace)
        {
            using var connection = BuildConnection();

            var cacheConfig = new DistributedCacheConfig
            {
                CacheName = "Test2",
                CacheType = "Redis",
                Host = "Here"
            };
            var mockTelemetryConfig = new TelemetryConfig { MillisecondThreshold = threshold };
            var mockTelemetry = new TelemetryProcessor();
            var cache = BuildDistributedCache(connection, useSerializer: false)
                .WithApplicationInsightsTelemetry(cacheConfig, mockTelemetry, mockTelemetryConfig);

            const int elementsToCache = 10;

            var keys = Enumerable.Range(1, elementsToCache).ToArray();
            var values = keys
                .Where(k => k % 2 == 0)
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();

            await cache.SetMany(1, values, TimeSpan.FromSeconds(1));

            Task.WaitAll();

            var trace = mockTelemetry.GetTrace();

            trace.Should().NotBeNull();

            if (anyTrace)
            {
                trace.Should().NotBeEmpty();
                trace.Count.Should().Be(1);
                var commandText = trace.First().Command;
                commandText.Should().Contain("StringSetAsync");
                commandText.Should().Contain("Keys");
                commandText.Split("Keys ").Length.Should().Be(2);
                var actualKeys = commandText.Split("Keys ")[1].Replace("'", "");
                actualKeys.Should().Contain(",");
                var actualKeyList = actualKeys.Split(",");
                actualKeyList.Length.Should().Be(elementsToCache / 2);
                var firstKey = actualKeyList.First();
                firstKey.Should().NotBeNullOrEmpty();
                firstKey.Should().Be("1.2");
            }
        }

        private static RedisConnection BuildConnection() => new RedisConnection(TestConnectionString.Value);
        
        private static RedisCache<int, int, TestClass> BuildRedisCache(
            RedisConnection connection,
            bool useFireAndForget = false,
            string keyPrefix = null,
            string nullValue = null,
            bool useSerializer = false,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
        {
            if (useSerializer)
            {
                return new RedisCache<int, int, TestClass>(
                    connection,
                    k => k.ToString(),
                    k => k.ToString(),
                    new ProtoBufSerializer<TestClass>(),
                    useFireAndForgetWherePossible: useFireAndForget,
                    keySeparator: "_",
                    keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                    nullValue: nullValue,
                    subscriber: connection,
                    keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
            }
            
            return new RedisCache<int, int, TestClass>(
                connection,
                k => k.ToString(),
                k => k.ToString(),
                v => v.ToString(),
                v => TestClass.Parse(v),
                useFireAndForgetWherePossible: useFireAndForget,
                keySeparator: "_",
                keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                nullValue: nullValue,
                subscriber: connection,
                keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
        }

        private static IDistributedCache<int, int, TestClass> BuildDistributedCache(
            RedisConnection connection,
            bool useFireAndForget = false,
            string keyPrefix = null,
            string nullValue = null,
            bool useSerializer = false,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
        {
            if (useSerializer)
            {
                return new RedisCache<int, int, TestClass>(
                    connection,
                    k => k.ToString(),
                    k => k.ToString(),
                    new ProtoBufSerializer<TestClass>(),
                    useFireAndForgetWherePossible: useFireAndForget,
                    keySeparator: "_",
                    keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                    nullValue: nullValue,
                    subscriber: connection,
                    keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
            }

            return new RedisCache<int, int, TestClass>(
                connection,
                k => k.ToString(),
                k => k.ToString(),
                v => v.ToString(),
                v => TestClass.Parse(v),
                useFireAndForgetWherePossible: useFireAndForget,
                keySeparator: "_",
                keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                nullValue: nullValue,
                subscriber: connection,
                keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
        }
    }
}