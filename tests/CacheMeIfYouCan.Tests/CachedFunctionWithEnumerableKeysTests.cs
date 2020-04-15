using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public partial class CachedFunctionWithEnumerableKeysTests
    {
        [Fact]
        public void Concurrent_CorrectValuesAreReturned()
        {
            var delay = TimeSpan.FromMilliseconds(100);
            
            Func<IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = async keys =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                return keys.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
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
                            var keys = Enumerable.Range(i, 10).ToList();
                            var values = await cachedFunction(keys).ConfigureAwait(false);
                            values.Select(kv => kv.Key).Should().BeEquivalentTo(keys);
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
                Func<T, Dictionary<int, int>> originalFunction = keys =>
                {
                    return keys.ToDictionary(k => k);
                };

                var cachedFunction = CachedFunctionFactory
                    .ConfigureFor(originalFunction)
                    .WithEnumerableKeys<T, Dictionary<int, int>, int, int>()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .WithLocalCache(new MockLocalCache<int, int>())
                    .Build();
                
                var results1 = cachedFunction(converter(Enumerable.Range(0, 100)));

                results1.Should().HaveCount(100);
                results1.Should().Match(values => values.All(kv => kv.Key == kv.Value));
                results1.Select(kv => kv.Key).OrderBy(k => k).Should().BeEquivalentTo(Enumerable.Range(0, 100));

                var results2 = cachedFunction(converter(Enumerable.Range(50, 100)));
                
                results2.Should().HaveCount(100);
                results2.Should().Match(values => values.All(kv => kv.Key == kv.Value));
                results2.Select(kv => kv.Key).OrderBy(k => k).Should().BeEquivalentTo(Enumerable.Range(50, 100));
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CustomRequestType_SucceedsIfConfigured(bool configureRequestConverter)
        {
            Func<SortedSet<int>, Dictionary<int, int>> originalFunction = keys => keys.ToDictionary(k => k);
            
            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<SortedSet<int>, Dictionary<int, int>, int, int>()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new MockLocalCache<int, int>());

            if (!configureRequestConverter)
            {
                Action action = () => config.Build();
                action.Should().Throw<Exception>();
                return;
            }
            
            var cachedFunction = config
                .WithRequestConverter(k => new SortedSet<int>(k))
                .Build();

            var results = cachedFunction(new SortedSet<int>(Enumerable.Range(0, 100)));
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
                Func<IEnumerable<int>, T> originalFunction = keys =>
                {
                    return converter(keys.ToDictionary(k => k));
                };

                var cachedFunction = CachedFunctionFactory
                    .ConfigureFor(originalFunction)
                    .WithEnumerableKeys<IEnumerable<int>, T, int, int>()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .WithLocalCache(new MockLocalCache<int, int>())
                    .Build();
                
                var results = cachedFunction(Enumerable.Range(0, 100));

                results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
                results.Select(kv => kv.Key).OrderBy(k => k).Should().BeEquivalentTo(Enumerable.Range(0, 100));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CustomResponseType_SucceedsIfConfigured(bool configureResponseConverter)
        {
            Func<IEnumerable<int>, SortedDictionary<int, int>> originalFunction = keys =>
            {
                return new SortedDictionary<int, int>(keys.ToDictionary(k => k));
            };
            
            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, SortedDictionary<int, int>, int, int>()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new MockLocalCache<int, int>());

            if (!configureResponseConverter)
            {
                Action action = () => config.Build();
                action.Should().Throw<Exception>();
                return;
            }
            
            var cachedFunction = config
                .WithResponseConverter(d => new SortedDictionary<int, int>(d))
                .Build();

            var results = cachedFunction(Enumerable.Range(0, 100));
            results.Should().Match(values => values.All(kv => kv.Key == kv.Value));
            results.Keys.Should().BeEquivalentTo(Enumerable.Range(0, 100));
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(500, false)]
        public async Task WithCancellationToken_ThrowsIfCancelled(int cancelAfter, bool shouldThrow)
        {
            var delay = TimeSpan.FromMilliseconds(200);
            
            Func<IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = async (keys, cancellationToken) =>
            {
                await Task.Delay(delay, cancellationToken);
                return keys.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(cancelAfter);
            
            Func<Task<Dictionary<int, int>>> func = () => cachedFunction(new[] { 1 }, cancellationTokenSource.Token);

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
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromMilliseconds(timeToLiveMs))
                .Build();

            cachedFunction(new[] { 1 });
            
            Thread.Sleep(timeToLiveMs / 2);
            
            cachedFunction(new[] { 1 });
            cache.HitsCount.Should().Be(1);
            
            Thread.Sleep(timeToLiveMs);

            cachedFunction(new[] { 1 });
            cache.HitsCount.Should().Be(1);
        }
        
        [Fact]
        public void WithTimeToLiveFactory_SetsTimeToLiveCorrectly()
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLiveFactory(keys => TimeSpan.FromMilliseconds(keys.First()))
                .Build();

            foreach (var key in new[] { 250, 500, 1000 })
                CheckTimeToLiveForKey(key);

            void CheckTimeToLiveForKey(int key)
            {
                cachedFunction(new[] { key });

                Thread.Sleep(TimeSpan.FromMilliseconds((double)key / 2));

                cache.TryGet(key, out _).Should().BeTrue();

                Thread.Sleep(TimeSpan.FromMilliseconds(key));

                cache.TryGet(key, out _).Should().BeFalse();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DisableCaching_WorksAsExpected(bool disableCaching)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .DisableCaching(disableCaching)
                .Build();

            cachedFunction(new[] { 1 });

            if (disableCaching)
            {
                cache.GetManyExecutionCount.Should().Be(0);
                cache.SetManyExecutionCount.Should().Be(0);
            }
            else
            {
                cache.GetManyExecutionCount.Should().Be(1);
                cache.SetManyExecutionCount.Should().Be(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FillMissingKeysWithConstantValue_WorksAsExpected(bool fillMissingKeys)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.Where(k => k % 2 == 0).ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (fillMissingKeys)
                config.FillMissingKeys(-1);

            var cachedFunction = config.Build();

            var values = cachedFunction(Enumerable.Range(0, 10));

            foreach (var key in Enumerable.Range(0, 10))
            {
                if (key % 2 == 0)
                {
                    values[key].Should().Be(key);
                }
                else
                {
                    values.TryGetValue(key, out var value).Should().Be(fillMissingKeys);
                    cache.TryGet(key, out var valueFromCache).Should().Be(fillMissingKeys);

                    if (fillMissingKeys)
                    {
                        value.Should().Be(-1);
                        valueFromCache.Should().Be(-1);
                    }
                }
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FillMissingKeysUsingValueFactory_WorksAsExpected(bool fillMissingKeys)
        {
            Func<IEnumerable<int>, ConcurrentDictionary<int, int>> originalFunction = keys =>
            {
                return new ConcurrentDictionary<int, int>(keys.Where(k => k % 2 == 0).ToDictionary(k => k));
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, ConcurrentDictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (fillMissingKeys)
                config.FillMissingKeys(k => -k);

            var cachedFunction = config.Build();

            var values = cachedFunction(Enumerable.Range(0, 10));

            foreach (var key in Enumerable.Range(0, 10))
            {
                if (key % 2 == 0)
                {
                    values[key].Should().Be(key);
                }
                else
                {
                    values.TryGetValue(key, out var value).Should().Be(fillMissingKeys);
                    cache.TryGet(key, out var valueFromCache).Should().Be(fillMissingKeys);

                    if (fillMissingKeys)
                    {
                        value.Should().Be(-key);
                        valueFromCache.Should().Be(-key);
                    }
                }
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void DontGetFromCacheWhen_DontStoreInCacheWhen_WorksForAllCombinations(bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromCacheWhen(key => key % 2 == 0);

            if (flag2)
                config.DontStoreInCacheWhen((key, _) => key % 3 == 0);

            if (flag3)
                config.DontGetFromCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.DontStoreInCacheWhen((key, _) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount);
                else
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount + 1);

                if (skipSet)
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount);
                else
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount + 1);

                previousGetManyExecutionCount = currentGetManyExecutionCount;
                previousSetManyExecutionCount = currentSetManyExecutionCount;
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void DontGetFromCacheWhen_WithExtraParam_WorksForAllCombinations(bool flag1, bool flag2)
        {
            // Func always returns 1
            // Cache always returns 2
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .DontStoreInCacheWhen(_ => true);

            if (flag1)
                config.DontGetFromCacheWhen(outerParam => outerParam % 2 == 0);

            if (flag2)
                config.DontGetFromCacheWhen((_, innerKey) => innerKey % 3 == 0);
            
            var cachedFunction = config.Build();

            // Fill cache with 2's
            cache.SetMany(Enumerable.Range(0, 100).ToDictionary(j => j, j => 2), TimeSpan.FromSeconds(100));

            var results = new Dictionary<(int OuterKey, int InnerKey), int>();
            
            for (var outerParam = 0; outerParam < 10; outerParam++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                foreach (var (innerKey, value) in cachedFunction(outerParam, Enumerable.Range(10 * innerKeyBatch, 10)))
                    results[(outerParam, innerKey)] = value;
            }
            
            foreach (var ((outerParam, innerKey), value) in results)
            {
                var shouldHaveSkippedCache = flag1 && outerParam % 2 == 0 || flag2 && innerKey % 3 == 0;
                value.Should().Be(shouldHaveSkippedCache ? 1 : 2);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 3, MemberType = typeof(BoolGenerator))]
        public void DontStoreInCacheWhen_WithExtraParam_WorksForAllCombinations(bool flag1, bool flag2, bool flag3)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (flag1)
                config.DontStoreInCacheWhen(outerParam => outerParam % 2 == 0);

            if (flag2)
                config.DontStoreInCacheWhen((_, innerKey, __) => innerKey % 3 == 0);
            
            if (flag3)
                config.DontStoreInCacheWhen((_, innerKey, __) => innerKey % 5 == 0);
            
            var cachedFunction = config.Build();

            for (var outerParam = 0; outerParam < 10; outerParam++)
            {
                for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
                    cachedFunction(outerParam, Enumerable.Range(10 * innerKeyBatch, 10));
                
                var valuesInCache = new Dictionary<int, int>();
                foreach (var (innerKey, value) in cache.GetMany(Enumerable.Range(0, 100).ToList()))
                    valuesInCache[innerKey] = value;
                
                for (var innerKey = 0; innerKey < 100; innerKey++)
                {
                    var shouldHaveSkippedCache =
                        flag1 && outerParam % 2 == 0 ||
                        flag2 && innerKey % 3 == 0 ||
                        flag3 && innerKey % 5 == 0;

                    valuesInCache.ContainsKey(innerKey).Should().Be(!shouldHaveSkippedCache);
                }
                
                cache.Clear();
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 8, MemberType = typeof(BoolGenerator))]
        public void DontGetFromOrStoreInLocalCacheWhen_DontGetFromOrStoreInDistributedCacheWhen_WorksAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4, bool flag5, bool flag6, bool flag7, bool flag8)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.DontGetFromLocalCacheWhen(key => key % 2 == 0);

            if (flag2)
                config.DontStoreInLocalCacheWhen((key, _) => key % 3 == 0);

            if (flag3)
                config.DontGetFromLocalCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.DontStoreInLocalCacheWhen((key, _) => key % 7 == 0);
            
            if (flag5)
                config.DontGetFromDistributedCacheWhen(key => key % 11 == 0);

            if (flag6)
                config.DontStoreInDistributedCacheWhen((key, _) => key % 13 == 0);

            if (flag7)
                config.DontGetFromDistributedCacheWhen(key => key % 17 == 0);
            
            if (flag8)
                config.DontStoreInDistributedCacheWhen((key, _) => key % 19 == 0);
            
            var cachedFunction = config.Build();

            const int cycleLength = 2 * 3 * 5 * 7 * 11 * 13 * 17 * 19;
            
            var previousLocalGetManyExecutionCount = 0;
            var previousLocalSetManyExecutionCount = 0;
            var previousDistributedGetManyExecutionCount = 0;
            var previousDistributedSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i, i + cycleLength, i + (2 * cycleLength), i + (3 * cycleLength) });

                var currentLocalGetManyExecutionCount = localCache.GetManyExecutionCount;
                var currentLocalSetManyExecutionCount = localCache.SetManyExecutionCount;
                var currentDistributedGetManyExecutionCount = distributedCache.GetManyExecutionCount;
                var currentDistributedSetManyExecutionCount = distributedCache.SetManyExecutionCount;

                var skipLocalGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipLocalSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                var skipDistributedGet = flag5 && i % 11 == 0 || flag7 && i % 17 == 0;
                var skipDistributedSet = flag6 && i % 13 == 0 || flag8 && i % 19 == 0;
                
                if (skipLocalGet)
                    currentLocalGetManyExecutionCount.Should().Be(previousLocalGetManyExecutionCount);
                else
                    currentLocalGetManyExecutionCount.Should().Be(previousLocalGetManyExecutionCount + 1);

                if (skipLocalSet)
                    currentLocalSetManyExecutionCount.Should().Be(previousLocalSetManyExecutionCount);
                else
                    currentLocalSetManyExecutionCount.Should().Be(previousLocalSetManyExecutionCount + 1);

                if (skipDistributedGet)
                    currentDistributedGetManyExecutionCount.Should().Be(previousDistributedGetManyExecutionCount);
                else
                    currentDistributedGetManyExecutionCount.Should().Be(previousDistributedGetManyExecutionCount + 1);

                if (skipDistributedSet)
                    currentDistributedSetManyExecutionCount.Should().Be(previousDistributedSetManyExecutionCount);
                else
                    currentDistributedSetManyExecutionCount.Should().Be(previousDistributedSetManyExecutionCount + 1);
                
                previousLocalGetManyExecutionCount = currentLocalGetManyExecutionCount;
                previousLocalSetManyExecutionCount = currentLocalSetManyExecutionCount;
                previousDistributedGetManyExecutionCount = currentDistributedGetManyExecutionCount;
                previousDistributedSetManyExecutionCount = currentDistributedSetManyExecutionCount;
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void DontGetFromOrStoreInLocalCacheWhen_ForSingleTierCache_ForSingleTierCache(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromLocalCacheWhen(key => key % 2 == 0);

            if (flag2)
                config.DontStoreInLocalCacheWhen((key, _) => key % 3 == 0);

            if (flag3)
                config.DontGetFromLocalCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.DontStoreInLocalCacheWhen((key, _) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount);
                else
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount + 1);

                if (skipSet)
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount);
                else
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount + 1);

                previousGetManyExecutionCount = currentGetManyExecutionCount;
                previousSetManyExecutionCount = currentSetManyExecutionCount;
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void DontGetFromOrStoreInDistributedCacheWhen_ForSingleTierCache_ForSingleTierCache(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithDistributedCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromDistributedCacheWhen(key => key % 2 == 0);

            if (flag2)
                config.DontStoreInDistributedCacheWhen((key, _) => key % 3 == 0);

            if (flag3)
                config.DontGetFromDistributedCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.DontStoreInDistributedCacheWhen((key, _) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount);
                else
                    currentGetManyExecutionCount.Should().Be(previousGetManyExecutionCount + 1);

                if (skipSet)
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount);
                else
                    currentSetManyExecutionCount.Should().Be(previousSetManyExecutionCount + 1);

                previousGetManyExecutionCount = currentGetManyExecutionCount;
                previousSetManyExecutionCount = currentSetManyExecutionCount;
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void WithTimeToLiveFactory_KeysPassedIntoFunctionAreOnlyThoseThatWillBeStoredInCache(bool flag1, bool flag2)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .DontGetFromCacheWhen(_ => true)
                .WithTimeToLiveFactory(ValidateTimeToLiveFactoryKeys);

            if (flag1)
                config.DontStoreInCacheWhen((_, __) => true);

            if (flag2)
                config.DontStoreInCacheWhen((key, _) => key % 2 == 0);

            var cachedFunction = config.Build();

            cachedFunction(Enumerable.Range(0, 100));

            TimeSpan ValidateTimeToLiveFactoryKeys(IReadOnlyCollection<int> keys)
            {
                if (flag1)
                    throw new Exception();

                var expectedKeys = flag2
                    ? Enumerable.Range(0, 50).Select(i => (2 * i) + 1)
                    : Enumerable.Range(0, 100);

                keys.Should().BeEquivalentTo(expectedKeys);
                
                return TimeSpan.FromSeconds(1);
            }
        }

        [Theory]
        [InlineData(BatchBehaviour.FillBatchesEvenly, 8, 8, 9)]
        [InlineData(BatchBehaviour.FillEachBatchBeforeStartingNext, 10, 10, 5)]
        public void WithBatchedFetches_BatchesFetchesCorrectly(BatchBehaviour batchBehaviour, params int[] expectedBatchSizes)
        {
            var batchSizes = new List<int>();
            
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                batchSizes.Add(keys.Count());
                return keys.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(new MockLocalCache<int, int>())
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithBatchedFetches(10, batchBehaviour)
                .Build();

            cachedFunction(Enumerable.Range(0, 25)).Keys.Should().BeEquivalentTo(Enumerable.Range(0, 25));

            batchSizes.Should().BeEquivalentTo(expectedBatchSizes);
        }
        
        [Fact]
        public void OnResult_EventsTriggeredAsExpected()
        {
            var cache = new MockLocalCache<int, int>();
            SuccessfulRequestEvent<string, int, int> lastSuccess = default;
            ExceptionEvent<string, int> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            var keys = Enumerable.Range(1, 10).ToList();
            var expectedResponse = keys.ToDictionary(k => k);

            Func<string, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2) =>
            {
                ThrowIfFirst();
                return p2.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<string, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .OnResult(r => lastSuccess = r, ex => lastException = ex)
                .Build();

            Func<Dictionary<int, int>> func = () => cachedFunction("abc", keys);
            func.Should().Throw<Exception>();
            CheckException();
            func().Should().BeEquivalentTo(expectedResponse);
            CheckSuccess(false);
            func().Should().BeEquivalentTo(expectedResponse);
            CheckSuccess(true);

            void ThrowIfFirst()
            {
                if (!first)
                    return;

                first = false;
                throw new Exception(exceptionMessage);
            }

            void CheckSuccess(bool wasCached)
            {
                var now = DateTime.UtcNow;
                lastSuccess.Parameters.Should().Be("abc");
                lastSuccess.Keys.Should().BeEquivalentTo(keys);
                lastSuccess.Values.Should().BeEquivalentTo(expectedResponse);
                lastSuccess.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastSuccess.CacheHits.Should().Be(wasCached ? keys.Count : 0);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Should().Be("abc");
                lastException.Keys.Should().BeEquivalentTo(keys);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
    }
}