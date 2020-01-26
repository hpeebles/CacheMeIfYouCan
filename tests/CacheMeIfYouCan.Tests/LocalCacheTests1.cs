using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.LocalCaches;
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
            var cache = BuildCache(cacheName);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var key = 2 * ((10 * i) + j);
                        
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
            var cache = BuildCache(cacheName);
            
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
            var cache = BuildCache(cacheName);

            var tasks = Enumerable
                .Range(1, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var keys = Enumerable.Range((10 * i) + j, i).ToList();
                        cache.SetMany(keys.Select(k => new KeyValuePair<int, int>(k, k)).ToList(), TimeSpan.FromSeconds(1));
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
            var cache = BuildCache(cacheName);

            for (var i = 0; i < 10; i++)
            {
                cache.Set(i, i, TimeSpan.FromSeconds(1));
                cache.TryRemove(i, out var value).Should().BeTrue();
                value.Should().Be(i);
                cache.TryRemove(i, out _).Should().BeFalse();
            }
        }

        private static ILocalCache<int, int> BuildCache(string cacheName)
        {
            return cacheName switch
            {
                MemoryCache => (ILocalCache<int, int>) new MemoryCache<int, int>(k => k.ToString()),
                DictionaryCache => new DictionaryCache<int, int>(),
                _ => throw new Exception($"No! Stop being silly! {cacheName} is not valid cache name")
            };
        }
    }
}