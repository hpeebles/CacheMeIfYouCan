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
    /// Tests for <see cref="RedisCache{TKey,TValue}"/>
    /// </summary>
    public class RedisCacheTests1
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Concurrent_Set_TryGet_AllItemsReturnedSuccessfully(bool useSerializer)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, useSerializer: useSerializer);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => Task.Run(async () =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var key = 2 * ((1000 * i) + j);
                        
                        await cache.Set(key, new TestClass(key), TimeSpan.FromSeconds(1));
                        var (success, value) = await cache.TryGet(key);
                        success.Should().BeTrue();
                        value.Value.IntValue.Should().Be(key);
                        value.Value.StringValue.Should().Be(key.ToString());
                        value.TimeToLive.Should().BePositive().And.BeLessThan(TimeSpan.FromSeconds(1));
                        (await cache.TryGet(key + 1)).Success.Should().BeFalse();
                        Thread.Yield();
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }
        
        [Fact]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);

            var tasks = Enumerable
                .Range(1, 5)
                .Select(i => Task.Run(async () =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var keys = Enumerable.Range((10 * i) + j, i).ToArray();
                        await cache.SetMany(keys.Select(k => new KeyValuePair<int, TestClass>(k, new TestClass(k))).ToArray(), TimeSpan.FromSeconds(1));
                        var values = await cache.GetMany(keys);
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

            await cache.SetMany(values, TimeSpan.FromSeconds(1));

            var fromCache = await cache.GetMany(keys);

            fromCache.Select(kv => kv.Key).Should().BeEquivalentTo(values.Select(kv => kv.Key));
        }
        
        [Theory]
        [InlineData(50)]
        [InlineData(1000)]
        public async Task Set_WithTimeToLive_DataExpiredCorrectly(int timeToLiveMs)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            await cache.Set(1, new TestClass(1), TimeSpan.FromMilliseconds(timeToLiveMs));
            (await cache.TryGet(1)).Success.Should().BeTrue();
            
            Thread.Sleep(timeToLiveMs + 20);

            (await cache.TryGet(1)).Success.Should().BeFalse();
        }
        
        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task SetMany_WithTimeToLive_DataExpiredCorrectly(int timeToLiveMs)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            await cache.SetMany(new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromMilliseconds(timeToLiveMs));
            (await cache.GetMany(new[] { 1 })).Should().ContainSingle();
            
            Thread.Sleep(timeToLiveMs + 20);

            (await cache.GetMany(new[] { 1 })).Should().BeEmpty();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void WhenFireAndForgetEnabled_SetOperationsCompleteImmediately(bool setMany, bool useFireAndForget)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, useFireAndForget);

            var task = setMany
                ? cache.Set(1, new TestClass(1), TimeSpan.FromSeconds(1))
                : cache.SetMany(new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromSeconds(1));

            task.IsCompleted.Should().Be(useFireAndForget);
        }

        [Fact]
        public async Task WhenDisposed_ThrowsObjectDisposedExceptionIfAccessed()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            cache.Dispose();

            Func<Task> task = () => cache.TryGet(1);

            await task.Should().ThrowExactlyAsync<ObjectDisposedException>().WithMessage($"* '{cache.GetType()}'.");
        }

        [Fact]
        public async Task WhenConnectionDisposed_ThrowsObjectDisposedExceptionIfAccessed()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);
            
            connection.Dispose();

            Func<Task> task = () => cache.TryGet(1);

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
            
            await cache.Set(1, null, TimeSpan.FromSeconds(1));
            var (success, value) = await cache.TryGet(1);
            success.Should().BeTrue();
            value.Value.Should().BeNull();
            value.TimeToLive.Should().BePositive().And.BeLessThan(TimeSpan.FromSeconds(1));

            var rawValueInRedis = await redisDb.StringGetAsync(keyPrefix + "1");
            rawValueInRedis.Should().Be(nullValue);
        }

        [Fact]
        public async Task TryRemove_WorksAsExpected()
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection);

            var keys = Enumerable
                .Range(0, 10)
                .ToArray();
            
            var values = keys
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();

            await cache.SetMany(values, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            for (var i = 5; i < 15; i++)
                (await cache.TryRemove(i).ConfigureAwait(false)).Should().Be(i < 10);

            var fromCache = await cache
                .GetMany(keys)
                .ConfigureAwait(false);

            fromCache.Select(kv => kv.Key).Should().BeEquivalentTo(keys.Take(5));
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

            var expectedEvents = new List<(string, KeyEventType)>();
            
            if (eventTypes.HasFlag(KeyEventType.Set))
                expectedEvents.AddRange(values.Select(kv => (kv.Key.ToString(), KeyEventType.Set)));
            
            if (eventTypes.HasFlag(KeyEventType.Del))
                expectedEvents.Add(("1", KeyEventType.Del));
            
            if (eventTypes.HasFlag(KeyEventType.Expired))
                expectedEvents.AddRange(values.Skip(2).Select(kv => (kv.Key.ToString(), KeyEventType.Expired)));

            expectedEvents = expectedEvents
                .OrderBy(t => t.Item1)
                .ThenBy(t => t.Item2)
                .ToList();
            
            using var countdown = new CountdownEvent(expectedEvents.Count);
            var keysChangedRemotely = new ConcurrentBag<(string, KeyEventType)>();

            using var _ = cache1.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely.Add((t.Key, t.EventType));
                countdown.Signal();
            });
            
            await cache1
                .SetMany(values, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);

            keysChangedRemotely.Should().BeEmpty();
            
            await cache2
                .SetMany(values.ToArray(), TimeSpan.FromMilliseconds(500))
                .ConfigureAwait(false);

            await cache1.TryRemove(0).ConfigureAwait(false);
            await cache2.TryRemove(1).ConfigureAwait(false);

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
            
            var keysChangedRemotely1 = new ConcurrentBag<(string, KeyEventType)>(); 
            var keysChangedRemotely2 = new ConcurrentBag<(string, KeyEventType)>(); 

            using var _ = cache1.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely1.Add((t.Key, t.EventType));
                countdown.Signal();
            });
            
            using var __ = cache2.KeysChangedRemotely.Subscribe(t =>
            {
                keysChangedRemotely2.Add((t.Key, t.EventType));
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
                .SetMany(values1, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            await cache4
                .SetMany(values2, TimeSpan.FromSeconds(10))
                .ConfigureAwait(false);

            countdown.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();

            var expectedEvents1 = values1.Select(kv => (kv.Key.ToString(), KeyEventType.Set)).ToArray();
            var expectedEvents2 = values2.Select(kv => (kv.Key.ToString(), KeyEventType.Set)).ToArray();
            
            keysChangedRemotely1.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Should().BeEquivalentTo(expectedEvents1);
            keysChangedRemotely2.OrderBy(t => t.Item1).ThenBy(t => t.Item2).Should().BeEquivalentTo(expectedEvents2);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1000, false)]
        public async Task TestTelemetryOnCommand(int threshold, bool anyTrace)
        {
            using var connection = BuildConnection();
            using var cache = BuildRedisCache(connection, useSerializer: false);

            var mockTelemetry = new MockTelemetryProcessor();
            var config = new MockTelemetryConfig(threshold);

            cache.WithAppInsightsTelemetry(mockTelemetry, config, "host", "TestCache1");

            const int elementsToCache = 10;

            var keys = Enumerable
                .Range(1, elementsToCache)
                .ToArray();

            var values = keys
                .Select(i => new KeyValuePair<int, TestClass>(i, new TestClass(i)))
                .ToArray();

            await cache.SetMany(values, TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            Task.WaitAll();

            var trace = mockTelemetry.GetTrace();

            trace.Should().NotBeNull();

            if (anyTrace)
            {
                trace.Should().NotBeEmpty();
                trace.Count.Should().BeLessOrEqualTo(elementsToCache);
                trace.First().Command.Should().Contain("StringSetAsync");
                trace.Last().Command.Should().Contain("StringSetAsync");
            }
        }


        private static RedisConnection BuildConnection() => new RedisConnection(TestConnectionString.Value);

        private static RedisCache<int, TestClass> BuildRedisCache(
            RedisConnection connection,
            bool useFireAndForget = false,
            string keyPrefix = null,
            string nullValue = null,
            bool useSerializer = false,
            KeyEventType keyEventTypesToSubscribeTo = KeyEventType.None)
        {
            if (useSerializer)
            {
                return new RedisCache<int, TestClass>(
                    connection,
                    k => k.ToString(),
                    new ProtoBufSerializer<TestClass>(),
                    useFireAndForgetWherePossible: useFireAndForget,
                    keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                    nullValue: nullValue,
                    subscriber: connection,
                    keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
            }
            
            return new RedisCache<int, TestClass>(
                connection,
                k => k.ToString(),
                v => v.ToString(),
                v => TestClass.Parse(v),
                useFireAndForgetWherePossible: useFireAndForget,
                keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                nullValue: nullValue,
                subscriber: connection,
                keyEventTypesToSubscribeTo: keyEventTypesToSubscribeTo);
        }
    }
}