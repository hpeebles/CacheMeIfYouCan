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
                await Task.Delay(delay);
                return keys.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
                            var values = await cachedFunction(keys);
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
                    .ConfigureFor<int, int, T, Dictionary<int, int>>(originalFunction)
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
                .ConfigureFor<int, int, SortedSet<int>, Dictionary<int, int>>(originalFunction)
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
                    .ConfigureFor<int, int, IEnumerable<int>, T>(originalFunction)
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
                .ConfigureFor<int, int, IEnumerable<int>, SortedDictionary<int, int>>(originalFunction)
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
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(cancelAfter);
            
            Func<Task<Dictionary<int, int>>> func = () => cachedFunction(new[] { 1 }, cancellationTokenSource.Token);

            if (shouldThrow)
                await func.Should().ThrowAsync<OperationCanceledException>();
            else
                await func.Should().NotThrowAsync();
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
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void SkipCacheGet_SkipCacheSet_WorksForAllCombinations(bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<IEnumerable<int>, Dictionary<int, int>> originalFunction = keys =>
            {
                return keys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
            Func<IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = keys =>
            {
                return Task.FromResult(keys.ToDictionary(k => k));
            };

            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor<int, int, IEnumerable<int>, Dictionary<int, int>>(originalFunction)
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
    }
}