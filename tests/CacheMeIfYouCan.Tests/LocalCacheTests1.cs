using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="ILocalCache{TKey,TValue}" />
    /// </summary>
    public class LocalCacheTests1
    {
        private const string MemoryCache = nameof(MemoryCache);
        private const string DictionaryCache = nameof(DictionaryCache);
        
        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Concurrent_Set_TryGet_AllItemsReturnedSuccessfully(string cacheName)
        {
            var cache = BuildCache<int, int>(cacheName);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var key = 2 * ((1000 * i) + j);
                        
                        cache.Set(key, key, TimeSpan.FromSeconds(1));
                        cache.TryGet(key, out var value).Should().BeTrue();
                        value.Should().Be(key);
                        cache.TryGet(key + 1, out _).Should().BeFalse();
                        Thread.Yield();
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }

        [Theory]
        [InlineData(MemoryCache, 50)]
        [InlineData(MemoryCache, 1000)]
        [InlineData(DictionaryCache, 50)]
        [InlineData(DictionaryCache, 1000)]
        public void WithTimeToLive_DataExpiredCorrectly(string cacheName, int timeToLiveMs)
        {
            var cache = BuildCache<int, int>(cacheName);
            
            cache.Set(1, 1, TimeSpan.FromMilliseconds(timeToLiveMs));
            cache.TryGet(1, out _).Should().BeTrue();
            
            Thread.Sleep(timeToLiveMs + 20);

            cache.TryGet(1, out _).Should().BeFalse();
        }
        
        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully(string cacheName)
        {
            var cache = BuildCache<int, int>(cacheName);

            var tasks = Enumerable
                .Range(1, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var keys = Enumerable.Range((10 * i) + j, i).ToArray();
                        cache.SetMany(keys.Select(k => new KeyValuePair<int, int>(k, k)).ToArray(), TimeSpan.FromSeconds(1));
                        var values = cache.GetMany(keys);
                        values.Select(kv => kv.Key).Should().BeEquivalentTo(keys);
                        values.Select(kv => kv.Value).Should().BeEquivalentTo(keys);
                        Thread.Yield();
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }

        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void TryRemove_WorksAsExpected(string cacheName)
        {
            var cache = BuildCache<int, int>(cacheName);

            for (var i = 0; i < 10; i++)
            {
                cache.Set(i, i, TimeSpan.FromSeconds(1));
                cache.TryRemove(i, out var value).Should().BeTrue();
                value.Should().Be(i);
                cache.TryRemove(i, out _).Should().BeFalse();
            }
        }

        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Count_WorksAsExpected(string cacheName)
        {
            var cache = BuildCache<int, int>(cacheName);

            cache.Count.Should().Be(0);

            for (var i = 1; i < 10; i++)
            {
                cache.Set(i, i, TimeSpan.FromSeconds(1));
                cache.Count.Should().Be(i);
            }
            
            for (var i = 9; i > 0; i--)
            {
                cache.Count.Should().Be(i);
                cache.TryRemove(i, out _).Should().BeTrue();
            }

            cache.Count.Should().Be(0);
        }
        
        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Clear_WorksAsExpected(string cacheName)
        {
            var cache = BuildCache<int, int>(cacheName);

            for (var i = 1; i < 10; i++)
                cache.Set(i, i, TimeSpan.FromSeconds(1));

            cache.Clear();
            cache.Count.Should().Be(0);
            
            for (var i = 1; i < 10; i++)
                cache.TryGet(i, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Set_Get_TryRemove_WithNullValue_ValueIsCached(string cacheName)
        {
            var cache = BuildCache<int, string>(cacheName);

            cache.Set(1, null, TimeSpan.FromSeconds(1));
            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().BeNull();

            cache.TryRemove(1, out value).Should().BeTrue();
            value.Should().BeNull();

            cache.TryRemove(1, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void SetMany_GetMany_WithNullValue_ValueIsCached(string cacheName)
        {
            var cache = BuildCache<int, string>(cacheName);

            cache.SetMany(new[] { new KeyValuePair<int, string>(1, null) }, TimeSpan.FromSeconds(1));
            cache.GetMany(new[] { 1 }).Single().Value.Should().BeNull();
        }

        private static ILocalCache<TKey, TValue> BuildCache<TKey, TValue>(string cacheName)
        {
            return cacheName switch
            {
                MemoryCache => new MemoryCache<TKey, TValue>(k => k.ToString()),
                DictionaryCache => new DictionaryCache<TKey, TValue>(),
                _ => throw new Exception($"No! Stop being silly! {cacheName} is not valid cache name")
            };
        }
    }
}