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
    public class LocalCacheTests
    {
        [Fact]
        public void Concurrent_Set_TryGet_AllItemsReturnedSuccessfully()
        {
            var cache = new MemoryCache<int, int>(k => k.ToString());

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < 100; j++)
                    {
                        var key = (10 * i) + j;
                        
                        cache.Set(key, key, TimeSpan.FromSeconds(1));
                        cache.TryGet(key, out var value).Should().BeTrue();
                        value.Should().Be(key);
                        Thread.Yield();
                    }
                }))
                .ToArray();

            Task.WaitAll(tasks);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(1000)]
        public void WithTimeToLive_DataExpiredCorrectly(int timeToLiveMs)
        {
            var cache = new MemoryCache<int, int>(k => k.ToString());
            
            cache.Set(1, 1, TimeSpan.FromMilliseconds(timeToLiveMs));
            cache.TryGet(1, out _).Should().BeTrue();
            
            Thread.Sleep(timeToLiveMs + 20);

            cache.TryGet(1, out _).Should().BeFalse();
        }
        
        [Fact]
        public void Concurrent_SetMany_GetMany_AllItemsReturnedSuccessfully()
        {
            var cache = new MemoryCache<int, int>(k => k.ToString());

            var tasks = Enumerable
                .Range(0, 5)
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
    }
}