using System;
using System.Collections.Generic;
using System.Linq;
using CacheMeIfYouCan.Internal;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    /// <summary>
    /// Tests for <see cref="TwoTierCache{TOuterKey,TInnerKey,TValue}"/>
    /// </summary>
    public class TwoTierCacheTests2
    {
        [Fact]
        public void GetMany_ChecksLocalCacheThenDistributedCache()
        {
            var localCache = new MockLocalCache<int, int, int>();
            var distributedCache = new MockDistributedCache<int, int, int>();

            var twoTierCache = new TwoTierCache<int, int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            twoTierCache.GetMany(1, Enumerable.Range(0, 100).ToList()).Result.Should().BeEmpty();
            localCache.GetManyExecutionCount.Should().Be(1);
            distributedCache.GetManyExecutionCount.Should().Be(1);
            
            twoTierCache.SetMany(1, Enumerable.Range(0, 50).Select(i => new KeyValuePair<int, int>(i, i)).ToList(), TimeSpan.FromSeconds(1));
            localCache.SetMany1ExecutionCount.Should().Be(1);
            distributedCache.SetManyExecutionCount.Should().Be(1);

            var results1 = twoTierCache.GetMany(1, Enumerable.Range(0, 100).ToList()).Result;
            results1.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));
            foreach (var (key, value) in results1)
                value.Should().Be(key);
            localCache.GetManyExecutionCount.Should().Be(2);
            distributedCache.GetManyExecutionCount.Should().Be(2);
            
            var results2 = twoTierCache.GetMany(1, Enumerable.Range(0, 50).ToList()).Result;
            results2.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));
            foreach (var (key, value) in results2)
                value.Should().Be(key);
            
            localCache.GetManyExecutionCount.Should().Be(3);
            distributedCache.GetManyExecutionCount.Should().Be(2);
        }
        
        [Fact]
        public void GetMany_SetsValueInLocalCacheIfFoundInDistributedCache()
        {
            var localCache = new MockLocalCache<int, int, int>();
            var distributedCache = new MockDistributedCache<int, int, int>();

            var twoTierCache = new TwoTierCache<int, int, int>(localCache, distributedCache, EqualityComparer<int>.Default);

            distributedCache
                .SetMany(1, Enumerable.Range(0, 50).Select(i => new KeyValuePair<int, int>(i, i)).ToList(), TimeSpan.FromSeconds(1))
                .Wait();
            
            distributedCache.SetManyExecutionCount.Should().Be(1);

            var values1 = twoTierCache.GetMany(1, Enumerable.Range(0, 100).ToList()).Result;

            values1.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));

            localCache.GetManyExecutionCount.Should().Be(1);
            localCache.SetMany2ExecutionCount.Should().Be(1);
            distributedCache.GetManyExecutionCount.Should().Be(1);

            var values2 = twoTierCache.GetMany(1, Enumerable.Range(0, 50).ToList()).Result;
            values2.Select(kv => kv.Key).Should().BeEquivalentTo(Enumerable.Range(0, 50));

            localCache.GetManyExecutionCount.Should().Be(2);
            distributedCache.GetManyExecutionCount.Should().Be(1);
        }
    }
}