using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationTests
    {
        [Fact]
        public void Concurrent_CorrectValuesAreReturned()
        {
            var delay = TimeSpan.FromMilliseconds(100);
            
            Func<int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = async (outerKey, innerKeys) =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(new MockLocalCache<int, int, int>())
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var timer = Stopwatch.StartNew();
            
            RunTasks();

            timer.Elapsed.Should().BeGreaterThan(delay);
            timer.Restart();
            
            RunTasks();

            // All values should be cached this time
            timer.Elapsed.Should().BeLessThan(delay);
            
            void RunTasks()
            {
                var tasks = Enumerable
                    .Range(0, 5)
                    .Select(async _ =>
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            var innerKeys = Enumerable.Range(i, 10).ToList();
                            var values = await cachedFunction(0, innerKeys).ConfigureAwait(false);
                            values.Select(kv => kv.Key).Should().BeEquivalentTo(innerKeys);
                        }
                    })
                    .ToArray();

                Task.WaitAll(tasks);
            }
        }

        [Theory]
        [InlineData("Array")]
        [InlineData("HashSet")]
        [InlineData("List")]
        public void DifferentRequestType_Succeeds(string typeName)
        {
            switch (typeName)
            {
                case "Array":
                    RunTest<int[]>(k => k.ToArray());
                    break;
                
                case "HashSet":
                    RunTest<HashSet<int>>(k => new HashSet<int>(k));
                    break;
                
                case "List":
                    RunTest<List<int>>(k => k.ToList());
                    break;
                
                default:
                    throw new Exception();
            }

            void RunTest<T>(Func<IEnumerable<int>, T> converter)
                where T : IEnumerable<int>
            {
                Func<int, T, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
                {
                    return innerKeys.ToDictionary(k => k, k => outerKey + k);
                };

                var cachedFunction = CachedFunctionFactory
                    .ConfigureFor<int, int, int, T, Dictionary<int, int>>(originalFunction)
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .WithLocalCache(new MockLocalCache<int, int, int>())
                    .Build();
                
                var results = cachedFunction(0, converter(Enumerable.Range(0, 100)));

                results.Should().HaveCount(100);
                results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
                results.Select(kv => kv.Key).OrderBy(k => k).Should().BeEquivalentTo(Enumerable.Range(0, 100));
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CustomRequestType_SucceedsIfConfigured(bool configureRequestConverter)
        {
            Func<int, SortedSet<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };
            
            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, SortedSet<int>, Dictionary<int, int>>(originalFunction)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new MockLocalCache<int, int, int>());

            if (!configureRequestConverter)
            {
                Action action = () => config.Build();
                action.Should().Throw<Exception>();
                return;
            }
            
            var cachedFunction = config
                .WithRequestConverter(k => new SortedSet<int>(k))
                .Build();

            var results = cachedFunction(0, new SortedSet<int>(Enumerable.Range(0, 100)));
            results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
            results.Keys.Should().BeEquivalentTo(Enumerable.Range(0, 100));
        }
        
        [Theory]
        [InlineData("ConcurrentDictionary")]
        [InlineData("ListKeyValuePair")]
        [InlineData("ArrayKeyValuePair")]
        public void DifferentResponseType_Succeeds(string typeName)
        {
            switch (typeName)
            {
                case "ConcurrentDictionary":
                    RunTest<ConcurrentDictionary<int, int>>(d => new ConcurrentDictionary<int, int>(d));
                    break;
                
                case "ListKeyValuePair":
                    RunTest<List<KeyValuePair<int, int>>>(d => d.ToList());
                    break;
                
                case "ArrayKeyValuePair":
                    RunTest<KeyValuePair<int, int>[]>(d => d.ToArray());
                    break;
                
                default:
                    throw new Exception();
            }
            
            void RunTest<T>(Func<Dictionary<int, int>, T> converter)
                where T : IEnumerable<KeyValuePair<int, int>>
            {
                Func<int, IEnumerable<int>, T> originalFunction = (outerKey, innerKeys) =>
                {
                    return converter(innerKeys.ToDictionary(k => k, k => outerKey + k));
                };

                var cachedFunction = CachedFunctionFactory
                    .ConfigureFor<int, int, int, IEnumerable<int>, T>(originalFunction)
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .WithLocalCache(new MockLocalCache<int, int, int>())
                    .Build();
                
                var results = cachedFunction(0, Enumerable.Range(0, 100));

                results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
                results.Select(kv => kv.Key).OrderBy(k => k).Should().BeEquivalentTo(Enumerable.Range(0, 100));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CustomResponseType_SucceedsIfConfigured(bool configureResponseConverter)
        {
            Func<int, IEnumerable<int>, SortedDictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return new SortedDictionary<int, int>(innerKeys.ToDictionary(k => k, k => outerKey + k));
            };
            
            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, SortedDictionary<int, int>>(originalFunction)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new MockLocalCache<int, int, int>());

            if (!configureResponseConverter)
            {
                Action action = () => config.Build();
                action.Should().Throw<Exception>();
                return;
            }
            
            var cachedFunction = config
                .WithResponseConverter(d => new SortedDictionary<int, int>(d))
                .Build();

            var results = cachedFunction(0, Enumerable.Range(0, 100));
            results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
            results.Keys.Should().BeEquivalentTo(Enumerable.Range(0, 100));
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(500, false)]
        public async Task WithCancellationToken_ThrowsIfCancelled(int cancelAfter, bool shouldThrow)
        {
            var delay = TimeSpan.FromMilliseconds(200);
            
            Func<int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = async (outerKey, innerKeys, cancellationToken) =>
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(new MockLocalCache<int, int, int>())
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(cancelAfter);
            
            Func<Task<Dictionary<int, int>>> func = () => cachedFunction(0, new[] { 1 }, cancellationTokenSource.Token);

            if (shouldThrow)
                await func.Should().ThrowAsync<OperationCanceledException>().ConfigureAwait(false);
            else
                await func.Should().NotThrowAsync().ConfigureAwait(false);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(250)]
        [InlineData(1000)]
        public void WithTimeToLive_SetsTimeToLiveCorrectly(int timeToLiveMs)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cache = new MockLocalCache<int, int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromMilliseconds(timeToLiveMs))
                .Build();

            cachedFunction(0, new[] { 1 });
            
            Thread.Sleep(timeToLiveMs / 2);
            
            cachedFunction(0, new[] { 1 });
            cache.HitsCount.Should().Be(1);
            
            Thread.Sleep(timeToLiveMs);

            cachedFunction(0, new[] { 1 });
            cache.HitsCount.Should().Be(1);
        }
        
        [Fact]
        public void WithTimeToLiveFactory_SetsTimeToLiveCorrectly()
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cache = new MockLocalCache<int, int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLiveFactory(outerKey => TimeSpan.FromMilliseconds(outerKey))
                .Build();

            foreach (var key in new[] { 250, 500, 1000 })
                CheckTimeToLiveForKey(key);

            void CheckTimeToLiveForKey(int key)
            {
                cachedFunction(0, new[] { key });

                Thread.Sleep(TimeSpan.FromMilliseconds(key / 2));

                cache.GetMany(0, new[] { key }).Should().BeNullOrEmpty();

                Thread.Sleep(TimeSpan.FromMilliseconds(key));

                cache.GetMany(0, new[] { key }).Should().BeNullOrEmpty();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisableCaching_WorksAsExpected(bool disableCaching)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cache = new MockLocalCache<int, int, int>();

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .DisableCaching(disableCaching)
                .Build();

            cachedFunction(0, new[] { 1 });

            if (disableCaching)
            {
                cache.GetManyExecutionCount.Should().Be(0);
                cache.SetMany1ExecutionCount.Should().Be(0);
            }
            else
            {
                cache.GetManyExecutionCount.Should().Be(1);
                cache.SetMany1ExecutionCount.Should().Be(1);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void SkipCacheGet_WorksForAllCombinations(bool flag1, bool flag2)
        {
            // Func always returns 1
            // Cache always returns 2
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.SkipCacheWhen(outerKey => outerKey % 2 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag2)
                config.SkipCacheWhen((outerKey, innerKey) => innerKey % 3 == 0, SkipCacheWhen.SkipCacheGet);
            
            var cachedFunction = config.Build();

            // Fill cache with 2's
            for (var outerKey = 0; outerKey < 10; outerKey++)
                cache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2), TimeSpan.FromSeconds(1));

            var results = new Dictionary<(int OuterKey, int InnerKey), int>();
            
            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                foreach (var (innerKey, value) in cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10)))
                    results[(outerKey, innerKey)] = value;
            }
            
            foreach (var ((outerKey, innerKey), value) in results)
            {
                var shouldHaveSkippedCache = flag1 && outerKey % 2 == 0 || flag2 && innerKey % 3 == 0;
                
                value.Should().Be(shouldHaveSkippedCache ? 1 : 2);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 3, MemberType = typeof(BoolGenerator))]
        public void SkipCacheSet_WorksForAllCombinations(bool flag1, bool flag2, bool flag3)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (flag1)
                config.SkipCacheWhen(outerKey => outerKey % 2 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag2)
                config.SkipCacheWhen((outerKey, innerKey) => innerKey % 3 == 0, SkipCacheWhen.SkipCacheSet);
            
            if (flag3)
                config.SkipCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10));
            }
            
            var valuesInCache = new Dictionary<(int, int), int>();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                foreach (var (innerKey, value) in cache.GetMany(outerKey, Enumerable.Range(0, 100).ToList()))
                    valuesInCache[(outerKey, innerKey)] = value;
            }

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKey = 0; innerKey < 100; innerKey++)
            {
                var shouldHaveSkippedCache =
                    flag1 && outerKey % 2 == 0 ||
                    flag2 && innerKey % 3 == 0 ||
                    flag3 && innerKey % 5 == 0;

                valuesInCache.ContainsKey((outerKey, innerKey)).Should().Be(!shouldHaveSkippedCache);
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void SkipLocalCacheGet_SkipDistributedCacheGet_WorkAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            // Func always returns 1
            // LocalCache always returns 2
            // DistributedCache always returns 3
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var localCache = new MockLocalCache<int, int, int>();
            var distributedCache = new MockDistributedCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.SkipLocalCacheWhen(outerKey => outerKey % 2 == 0, SkipCacheWhen.SkipCacheGet);
           
            if (flag2)
                config.SkipLocalCacheWhen((outerKey, innerKey) => innerKey % 3 == 0, SkipCacheWhen.SkipCacheGet);
            
            if (flag3)
                config.SkipDistributedCacheWhen(outerKey => outerKey % 5 == 0, SkipCacheWhen.SkipCacheGet);
            
            if (flag4)
                config.SkipDistributedCacheWhen((outerKey, innerKey) => innerKey % 7 == 0);
            
            var cachedFunction = config.Build();

            // Fill local cache with 2's and distributed cache with 3's
            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                localCache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2), TimeSpan.FromSeconds(1));
                distributedCache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 3), TimeSpan.FromSeconds(1));
            }
            
            var results = new Dictionary<(int OuterKey, int InnerKey), int>();
            
            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                foreach (var (innerKey, value) in cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10)))
                    results[(outerKey, innerKey)] = value;
            }
            
            foreach (var ((outerKey, innerKey), value) in results)
            {
                var shouldHaveSkippedLocalCache = flag1 && outerKey % 2 == 0 || flag2 && innerKey % 3 == 0;
                var shouldHaveSkippedDistributedCache = flag3 && outerKey % 5 == 0 || flag4 && innerKey % 7 == 0;

                var expectedValue = shouldHaveSkippedLocalCache
                    ? shouldHaveSkippedDistributedCache ? 1 : 3
                    : 2;
                
                value.Should().Be(expectedValue);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 6, MemberType = typeof(BoolGenerator))]
        public void SkipCacheSet_SkipDistributedCacheSet_WorkAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4, bool flag5, bool flag6)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var localCache = new MockLocalCache<int, int, int>();
            var distributedCache = new MockDistributedCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.SkipLocalCacheWhen(outerKey => outerKey % 2 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag2)
                config.SkipLocalCacheWhen((outerKey, innerKey) => innerKey % 3 == 0, SkipCacheWhen.SkipCacheSet);
           
            if (flag3)
                config.SkipLocalCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            if (flag4)
                config.SkipDistributedCacheWhen(outerKey => outerKey % 7 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag5)
                config.SkipDistributedCacheWhen((outerKey, innerKey) => innerKey % 11 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag6)
                config.SkipDistributedCacheWhen((outerKey, innerKey, value) => innerKey % 13 == 0);
            
            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10));
            }
            
            var valuesInLocalCache = new Dictionary<(int, int), int>();
            var valuesInDistributedCache = new Dictionary<(int, int), int>();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                foreach (var (innerKey, value) in localCache.GetMany(outerKey, Enumerable.Range(0, 100).ToList()))
                    valuesInLocalCache[(outerKey, innerKey)] = value;
                
                foreach (var (innerKey, value) in distributedCache.GetMany(outerKey, Enumerable.Range(0, 100).ToList()).Result)
                    valuesInDistributedCache[(outerKey, innerKey)] = value;
            }

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKey = 0; innerKey < 100; innerKey++)
            {
                var shouldHaveSkippedLocalCache =
                    flag1 && outerKey % 2 == 0 ||
                    flag2 && innerKey % 3 == 0 ||
                    flag3 && innerKey % 5 == 0;

                var shouldHaveSkippedDistributedCache =
                    flag4 && outerKey % 7 == 0 ||
                    flag5 && innerKey % 11 == 0 ||
                    flag6 && innerKey % 13 == 0;

                valuesInLocalCache.ContainsKey((outerKey, innerKey)).Should().Be(!shouldHaveSkippedLocalCache);
                valuesInDistributedCache.ContainsKey((outerKey, innerKey)).Should().Be(!shouldHaveSkippedDistributedCache);
            }
        }
    }
}