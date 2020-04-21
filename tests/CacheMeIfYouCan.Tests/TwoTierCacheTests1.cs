using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="TwoTierCache{TKey,TValue}"/>
    /// </summary>
    public class TwoTierCacheTests1
    {
        [Fact]
        public void TryGet_ChecksLocalCacheThenDistributedCache()
        {
            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var twoTierCache = new TwoTierCache<int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            for (var i = 1; i <= 100; i++)
            {
                twoTierCache.TryGet(i).Result.Success.Should().BeFalse();
                localCache.TryGetExecutionCount.Should().Be(i);
                distributedCache.TryGetExecutionCount.Should().Be(i);
            }

            for (var i = 1; i <= 100; i++)
            {
                twoTierCache.Set(i, i, TimeSpan.FromSeconds(1)).AsTask().Wait();
                localCache.SetExecutionCount.Should().Be(i);
                distributedCache.SetExecutionCount.Should().Be(i);
            }

            for (var i = 1; i <= 100; i++)
            {
                var (success, value, _) = twoTierCache.TryGet(i).Result;
                success.Should().BeTrue();
                value.Should().Be(i);

                localCache.TryGetExecutionCount.Should().Be(100 + i);
                distributedCache.TryGetExecutionCount.Should().Be(100);
            }
        }

        [Fact]
        public void TryGet_SetsValueInLocalCacheIfFoundInDistributedCache()
        {
            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var twoTierCache = new TwoTierCache<int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            for (var i = 1; i <= 100; i++)
            {
                distributedCache.Set(i, i, TimeSpan.FromSeconds(100));
                distributedCache.SetExecutionCount.Should().Be(i);
            }

            for (var i = 1; i <= 100; i++)
            {
                var (success, value, _) = twoTierCache.TryGet(i).Result;
                success.Should().BeTrue();
                value.Should().Be(i);
                localCache.SetExecutionCount.Should().Be(i);
                localCache.TryGet(i, out var fromLocalCache).Should().BeTrue();
                fromLocalCache.Should().Be(i);
            }
        }
        
        [Fact]
        public void GetMany_ChecksLocalCacheThenDistributedCache()
        {
            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var twoTierCache = new TwoTierCache<int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            twoTierCache.GetMany(Enumerable.Range(0, 100).ToList()).Result.Should().BeEmpty();
            localCache.GetManyExecutionCount.Should().Be(1);
            distributedCache.GetManyExecutionCount.Should().Be(1);
            
            twoTierCache.SetMany(Enumerable.Range(0, 50).Select(i => new KeyValuePair<int, int>(i, i)).ToList(), TimeSpan.FromSeconds(1));
            localCache.SetManyExecutionCount.Should().Be(1);
            distributedCache.SetManyExecutionCount.Should().Be(1);

            var results1 = twoTierCache.GetMany(Enumerable.Range(0, 100).ToList()).Result;
            results1.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));
            foreach (var (key, value) in results1)
                value.Should().Be(key);
            localCache.GetManyExecutionCount.Should().Be(2);
            distributedCache.GetManyExecutionCount.Should().Be(2);
            
            var results2 = twoTierCache.GetMany(Enumerable.Range(0, 50).ToList()).Result;
            results2.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));
            foreach (var (key, value) in results2)
                value.Should().Be(key);
            
            localCache.GetManyExecutionCount.Should().Be(3);
            distributedCache.GetManyExecutionCount.Should().Be(2);
        }
        
        [Fact]
        public void GetMany_SetsValueInLocalCacheIfFoundInDistributedCache()
        {
            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var twoTierCache = new TwoTierCache<int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            distributedCache
                .SetMany(Enumerable.Range(0, 50).Select(i => new KeyValuePair<int, int>(i, i)).ToList(), TimeSpan.FromSeconds(1))
                .Wait();
            
            distributedCache.SetManyExecutionCount.Should().Be(1);

            var values1 = twoTierCache.GetMany(Enumerable.Range(0, 100).ToList()).Result;

            values1.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));

            localCache.GetManyExecutionCount.Should().Be(1);
            localCache.SetExecutionCount.Should().Be(50);
            distributedCache.GetManyExecutionCount.Should().Be(1);

            var values2 = twoTierCache.GetMany(Enumerable.Range(0, 50).ToList()).Result;
            values2.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));

            localCache.GetManyExecutionCount.Should().Be(2);
            distributedCache.GetManyExecutionCount.Should().Be(1);
        }
    }
}