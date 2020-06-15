using System;
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
            using var cache = BuildRedisCache(useSerializer: useSerializer);

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
            using var cache = BuildRedisCache();

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
            using var cache = BuildRedisCache();

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
            using var cache = BuildRedisCache();
            
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
            using var cache = BuildRedisCache();
            
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
            using var cache = BuildRedisCache(useFireAndForget);

            var task = setMany
                ? cache.Set(1, new TestClass(1), TimeSpan.FromSeconds(1))
                : cache.SetMany(new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromSeconds(1));

            task.IsCompleted.Should().Be(useFireAndForget);
        }

        [Fact]
        public async Task WhenDisposed_ThrowsObjectDisposedExceptionIfAccessed()
        {
            var cache = BuildRedisCache();
            
            cache.Dispose();

            Func<Task> task = () => cache.TryGet(1);

            await task.Should().ThrowExactlyAsync<ObjectDisposedException>().WithMessage($"* '{cache.GetType()}'.");
        }

        [Theory]
        [InlineData("x")]
        [InlineData("000")]
        public async Task Nulls_StoredAsChosenNullValue(string nullValue)
        {
            var keyPrefix = Guid.NewGuid().ToString();
            
            using var cache = BuildRedisCache(keyPrefix: keyPrefix, nullValue: nullValue);

            var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value);
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
            var cache = BuildRedisCache();

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

        private static RedisCache<int, TestClass> BuildRedisCache(
            bool useFireAndForget = false,
            string keyPrefix = null,
            string nullValue = null,
            bool useSerializer = false)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            if (useSerializer)
            {
                return new RedisCache<int, TestClass>(
                    connectionMultiplexer,
                    k => k.ToString(),
                    new ProtoBufSerializer<TestClass>(),
                    useFireAndForgetWherePossible: useFireAndForget,
                    keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                    nullValue: nullValue);
            }
            
            return new RedisCache<int, TestClass>(
                connectionMultiplexer,
                k => k.ToString(),
                v => v.ToString(),
                v => TestClass.Parse(v),
                useFireAndForgetWherePossible: useFireAndForget,
                keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                nullValue: nullValue);
        }
    }
}