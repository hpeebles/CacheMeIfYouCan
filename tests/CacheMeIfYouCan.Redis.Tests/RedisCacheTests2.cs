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
    /// Tests for <see cref="RedisCache{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class RedisCacheTests2
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully(bool useSerializer)
        {
            var cache = BuildRedisCache(useSerializer: useSerializer);

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
            var cache = BuildRedisCache();

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
            var cache = BuildRedisCache();
            
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
            var cache = BuildRedisCache(useFireAndForget);

            var task = cache.SetMany(1, new[] { new KeyValuePair<int, TestClass>(1, new TestClass(1)) }, TimeSpan.FromSeconds(1));

            task.IsCompleted.Should().Be(useFireAndForget);
        }
        
        [Fact]
        public async Task WhenDisposed_ThrowsObjectDisposedException()
        {
            var cache = BuildRedisCache();
            
            cache.Dispose();

            Func<Task> task = () => cache.GetMany(1, new[] { 1 });

            await task.Should().ThrowExactlyAsync<ObjectDisposedException>().WithMessage($"* '{cache.GetType()}'.");
        }
        
        [Theory]
        [InlineData("x")]
        [InlineData("000")]
        public async Task Nulls_StoredAsChosenNullValue(string nullValue)
        {
            var keyPrefix = Guid.NewGuid().ToString();
            
            var cache = BuildRedisCache(keyPrefix: keyPrefix, nullValue: nullValue);

            var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value);
            var redisDb = connectionMultiplexer.GetDatabase();
            
            await cache.SetMany(1, new[] { new KeyValuePair<int, TestClass>(1, null) }, TimeSpan.FromSeconds(1));
            var values = await cache.GetMany(1, new[] { 1 });
            values.Should().ContainSingle();
            var (_, value) = values.Single();
            value.Value.Should().BeNull();
            value.TimeToLive.Should().BePositive().And.BeLessThan(TimeSpan.FromSeconds(1));

            var rawValueInRedis = await redisDb.StringGetAsync(keyPrefix + "11");
            rawValueInRedis.Should().Be(nullValue);
        }

        private static RedisCache<int, int, TestClass> BuildRedisCache(
            bool useFireAndForget = false,
            string keyPrefix = null,
            string nullValue = null,
            bool useSerializer = false)
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(TestConnectionString.Value);

            if (useSerializer)
            {
                return new RedisCache<int, int, TestClass>(
                    connectionMultiplexer,
                    k => k.ToString(),
                    k => k.ToString(),
                    new ProtoBufSerializer<TestClass>(),
                    useFireAndForgetWherePossible: useFireAndForget,
                    keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                    nullValue: nullValue);
            }
            
            return new RedisCache<int, int, TestClass>(
                connectionMultiplexer,
                k => k.ToString(),
                k => k.ToString(),
                v => v.ToString(),
                v => TestClass.Parse(v),
                useFireAndForgetWherePossible: useFireAndForget,
                keyPrefix: keyPrefix ?? Guid.NewGuid().ToString(),
                nullValue: nullValue);
        }
    }
}