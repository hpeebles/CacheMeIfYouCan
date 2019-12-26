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
    public class LocalCacheTests2
    {
        private const string MemoryCache = nameof(MemoryCache);
        private const string DictionaryCache = nameof(DictionaryCache);
        
        [Theory]
        [InlineData(MemoryCache)]
        [InlineData(DictionaryCache)]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully(string cacheName)
        {
            var cache = BuildCache(cacheName);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var keys = Enumerable.Range((10 * i) + j, i).ToList();
                        cache.SetMany(i, keys.Select(k => new KeyValuePair<int, int>(k, k)).ToList(), TimeSpan.FromSeconds(1));
                        var values = cache.GetMany(i, keys);
                        values.Select(kv => kv.Key).Should().BeEquivalentTo(keys);
                        values.Select(kv => kv.Value).Should().BeEquivalentTo(keys);
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
            
            cache.SetMany(1, new[] { new KeyValuePair<int, int>(1, 1) }, TimeSpan.FromMilliseconds(timeToLiveMs));
            cache.GetMany(1, new[] { 1 }).Should().ContainSingle();
            
            Thread.Sleep(timeToLiveMs + 20);

            cache.GetMany(1, new[] { 1 }).Should().BeNullOrEmpty();
        }

        private static ILocalCache<int, int, int> BuildCache(string cacheName)
        {
            return cacheName switch
            {
                MemoryCache => (ILocalCache<int, int, int>) new MemoryCache<int, int, int>(k => k.ToString(), k => k.ToString()),
                DictionaryCache => new DictionaryCache<int, int, int>(),
                _ => throw new Exception($"No! Stop being silly! {cacheName} is not valid cache name")
            };
        }
    }
}