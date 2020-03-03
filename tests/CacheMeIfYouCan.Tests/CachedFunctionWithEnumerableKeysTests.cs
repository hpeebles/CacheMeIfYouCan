using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.LocalCaches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class CachedFunctionWithEnumerableKeysTests
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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                    .ConfigureFor<T, Dictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<SortedSet<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                    .ConfigureFor<IEnumerable<int>, T, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, SortedDictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLiveFactory(keys => TimeSpan.FromMilliseconds(keys.First()))
                .Build();

            foreach (var key in new[] { 250, 500, 1000 })
                CheckTimeToLiveForKey(key);

            void CheckTimeToLiveForKey(int key)
            {
                cachedFunction(new[] { key });

                Thread.Sleep(TimeSpan.FromMilliseconds(key / 2));

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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
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
                .ConfigureFor<IEnumerable<int>, ConcurrentDictionary<int, int>, int, int>(originalFunction)
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
        public void SkipCacheGet_SkipCacheSet_WorksForAllCombinations(bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.SkipCacheWhen(key => key % 2 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag2)
                config.SkipCacheWhen(key => key % 3 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag3)
                config.SkipCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.SkipCacheWhen((key, value) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
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
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 8, MemberType = typeof(BoolGenerator))]
        public void SkipLocalCacheWhen_SkipDistributedCacheWhen_WorkAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4, bool flag5, bool flag6, bool flag7, bool flag8)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.SkipLocalCacheWhen(key => key % 2 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag2)
                config.SkipLocalCacheWhen(key => key % 3 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag3)
                config.SkipLocalCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.SkipLocalCacheWhen((key, value) => key % 7 == 0);
            
            if (flag5)
                config.SkipDistributedCacheWhen(key => key % 11 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag6)
                config.SkipDistributedCacheWhen(key => key % 13 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag7)
                config.SkipDistributedCacheWhen(key => key % 17 == 0);
            
            if (flag8)
                config.SkipDistributedCacheWhen((key, value) => key % 19 == 0);
            
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
                var skipLocalSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                var skipDistributedGet = flag5 && i % 11 == 0 || flag7 && i % 17 == 0;
                var skipDistributedSet = flag6 && i % 13 == 0 || flag7 && i % 17 == 0 || flag8 && i % 19 == 0;
                
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
        public void SkipLocalCacheWhen_ForSingleTierCache_WorksTheSameAsUsingSkipCacheWhen(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.SkipLocalCacheWhen(key => key % 2 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag2)
                config.SkipLocalCacheWhen(key => key % 3 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag3)
                config.SkipLocalCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.SkipLocalCacheWhen((key, value) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
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
        public void SkipDistributedCacheWhen_ForSingleTierCache_WorksTheSameAsUsingSkipCacheWhen(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithDistributedCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.SkipDistributedCacheWhen(key => key % 2 == 0, SkipCacheWhen.SkipCacheGet);

            if (flag2)
                config.SkipDistributedCacheWhen(key => key % 3 == 0, SkipCacheWhen.SkipCacheSet);

            if (flag3)
                config.SkipDistributedCacheWhen(key => key % 5 == 0);
            
            if (flag4)
                config.SkipDistributedCacheWhen((key, value) => key % 7 == 0);

            var cachedFunction = config.Build();

            var previousGetManyExecutionCount = 0;
            var previousSetManyExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(new[] { i });

                var currentGetManyExecutionCount = cache.GetManyExecutionCount;
                var currentSetManyExecutionCount = cache.SetManyExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
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
        public void WithTimeToLiveFactory_KeysPassedIntoFunctionAreOnlyThoseThatWontSkipCaching(bool flag1, bool flag2)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(cache)
                .SkipCacheWhen(x => true, SkipCacheWhen.SkipCacheGet)
                .WithTimeToLiveFactory(ValidateTimeToLiveFactoryKeys);

            if (flag1)
                config.SkipCacheWhen(key => true);

            if (flag2)
                config.SkipCacheWhen(key => key % 2 == 0);

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
                .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                .WithLocalCache(new MockLocalCache<int, int>())
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithBatchedFetches(10, batchBehaviour)
                .Build();

            cachedFunction(Enumerable.Range(0, 25)).Keys.Should().BeEquivalentTo(Enumerable.Range(0, 25));

            batchSizes.Should().BeEquivalentTo(expectedBatchSizes);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With1Param_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p, cancellationToken) => Task.FromResult(p.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = p => Task.FromResult(p.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p, cancellationToken) => p.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = p => p.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With2Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, cancellationToken) => Task.FromResult(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2) => Task.FromResult(p2.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, cancellationToken) => p2.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2) => p2.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With3Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, cancellationToken) => Task.FromResult(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3) => Task.FromResult(p3.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, cancellationToken) => p3.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3) => p3.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With4Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, cancellationToken) => Task.FromResult(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4) => Task.FromResult(p4.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => p4.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4) => p4.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With5Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => Task.FromResult(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5) => Task.FromResult(p5.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => p5.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5) => p5.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With6Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => Task.FromResult(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6) => Task.FromResult(p6.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => p6.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6) => p6.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With7Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => Task.FromResult(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => Task.FromResult(p7.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => p7.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => p7.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        public async Task With8Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            var input = Enumerable.Range(1, 10).ToArray();
            var expectedOutput = input.ToDictionary(x => x);
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => Task.FromResult(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, input, CancellationToken.None).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => Task.FromResult(p8.ToDictionary(x => x));
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, input).ConfigureAwait(false)).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, CancellationToken, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => p8.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input, CancellationToken.None).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => p8.ToDictionary(x => x);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor<int, int, int, int, int, int, int, IEnumerable<int>, Dictionary<int, int>, int, int>(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, input).Should().BeEquivalentTo(expectedOutput);
                    break;
                }
            }

            for (var i = 1; i <= 10; i++)
            {
                cache.TryGet(i, out var value).Should().BeTrue();
                value.Should().Be(i);
            }
        }
    }
}