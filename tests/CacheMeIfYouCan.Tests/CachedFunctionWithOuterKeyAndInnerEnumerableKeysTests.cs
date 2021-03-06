﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public partial class CachedFunctionWithOuterKeyAndInnerEnumerableKeysTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Concurrent_CorrectValuesAreReturned(bool useMemoryCache)
        {
            var delay = TimeSpan.FromMilliseconds(100);
            
            Func<int, IEnumerable<int>, Task<Dictionary<int, int>>> originalFunction = async (outerKey, innerKeys) =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (useMemoryCache)
                config.WithMemoryCache();
            else
                config.WithDictionaryCache();
            
            var cachedFunction = config.Build();

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
                    .ConfigureFor(originalFunction)
                    .WithEnumerableKeys<int, T, Dictionary<int, int>, int, int>()
                    .UseFirstParamAsOuterCacheKey()
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
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, SortedSet<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new MockLocalCache<int, int, int>());

            if (!configureRequestConverter)
            {
                Action action = () => config.Build();
                action.Should().Throw<Exception>();
                return;
            }
            
            var cachedFunction = config
                .WithRequestConverter(k => new SortedSet<int>(k.ToArray()))
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
                    .ConfigureFor(originalFunction)
                    .WithEnumerableKeys<int, IEnumerable<int>, T, int, int>()
                    .UseFirstParamAsOuterCacheKey()
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
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, SortedDictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
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

        [Fact]
        public async Task WithCancellationToken_CancellationPropagatesToUnderlyingFunction()
        {
            var wasCancelled = false;
            
            Func<int, IEnumerable<int>, CancellationToken, Task<Dictionary<int, int>>> originalFunction = async (outerKey, innerKeys, cancellationToken) =>
            {
                cancellationToken.Register(() => wasCancelled = true);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                return innerKeys.ToDictionary(k => k);
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            
            Func<Task<Dictionary<int, int>>> func = () => cachedFunction(1, new[] { 1 }, cancellationTokenSource.Token);

            await func.Should().ThrowExactlyAsync<TaskCanceledException>().ConfigureAwait(false);

            // Give the cancellation token time to complete the callback
            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            
            wasCancelled.Should().BeTrue();
        }

        [Theory]
        [InlineData(250)]
        [InlineData(500)]
        [InlineData(1000)]
        public void WithTimeToLive_SetsTimeToLiveCorrectly(int timeToLiveMs)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cache = new MockLocalCache<int, int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
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
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLiveFactory((outerKey, innerKeys) => TimeSpan.FromMilliseconds(outerKey))
                .Build();

            foreach (var key in new[] { 250, 500, 1000 })
                CheckTimeToLiveForKey(key);

            void CheckTimeToLiveForKey(int key)
            {
                cachedFunction(0, new[] { key });

                Thread.Sleep(TimeSpan.FromMilliseconds((double)key / 2));

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
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
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
        [InlineData(true)]
        [InlineData(false)]
        public void FillMissingKeysWithConstantValue_WorksAsExpected(bool fillMissingKeys)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.Where(k => k % 2 == 0).ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (fillMissingKeys)
                config.FillMissingKeys(-1);

            var cachedFunction = config.Build();

            var values = cachedFunction(1, Enumerable.Range(0, 10));

            foreach (var key in Enumerable.Range(0, 10))
            {
                if (key % 2 == 0)
                {
                    values[key].Should().Be(key);
                }
                else
                {
                    values.TryGetValue(key, out var value).Should().Be(fillMissingKeys);

                    var fromCache = cache.GetMany(1, new[] { key });
                    fromCache.Should().HaveCount(fillMissingKeys ? 1 : 0);
                    
                    if (fillMissingKeys)
                    {
                        value.Should().Be(-1);
                        fromCache.Single().Value.Should().Be(-1);
                    }
                }
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FillMissingKeysUsingValueFactory_WorksAsExpected(bool fillMissingKeys)
        {
            Func<int, IEnumerable<int>, ConcurrentDictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return new ConcurrentDictionary<int, int>(innerKeys.Where(k => k % 2 == 0).ToDictionary(k => k));
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, ConcurrentDictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (fillMissingKeys)
                config.FillMissingKeys((outerKey, innerKey) => -(outerKey + innerKey));

            var cachedFunction = config.Build();

            var values = cachedFunction(1, Enumerable.Range(0, 10));

            foreach (var key in Enumerable.Range(0, 10))
            {
                if (key % 2 == 0)
                {
                    values[key].Should().Be(key);
                }
                else
                {
                    values.TryGetValue(key, out var value).Should().Be(fillMissingKeys);

                    var fromCache = cache.GetMany(1, new[] { key });
                    fromCache.Should().HaveCount(fillMissingKeys ? 1 : 0);
                    
                    if (fillMissingKeys)
                    {
                        value.Should().Be(-(1 + key));
                        fromCache.Single().Value.Should().Be(-(1 + key));
                    }
                }
            }
        }
        
        [Fact]
        public void FilterResponseToWhere_WorksAsExpected()
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k);
            };

            var cache = new MockLocalCache<int, int, int>();
            SuccessfulRequestEvent<int, int, int, int> result = null;

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromMinutes(1))
                .FilterResponseToWhere((k, v) => v % 2 == 0)
                .OnResult(r => result = r);

            var cachedFunction = config.Build();

            var keys = Enumerable.Range(0, 10).ToArray();
            var values = cachedFunction(0, keys);

            values.Keys.Should().BeEquivalentTo(keys.Where(k => k % 2 == 0));
            cache.GetMany(0, keys).Select(kv => kv.Key).Should().BeEquivalentTo(keys);
            result.CountExcluded.Should().Be(keys.Length / 2);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void DontGetFromCacheWhen_WorksForAllCombinations(bool flag1, bool flag2)
        {
            // Func always returns 1
            // Cache always returns 2
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontGetFromCacheWhen((outerKey, innerKey) => innerKey % 3 == 0);
            
            var cachedFunction = config.Build();

            // Fill cache with 2's
            for (var outerKey = 0; outerKey < 10; outerKey++)
                cache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2).ToArray(), TimeSpan.FromSeconds(1));

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
        public void DontStoreInCacheWhen_WorksForAllCombinations(bool flag1, bool flag2, bool flag3)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (flag1)
                config.DontStoreInCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontStoreInCacheWhen((outerKey, innerKey, _) => innerKey % 3 == 0);
            
            if (flag3)
                config.DontStoreInCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10));
            }
            
            var valuesInCache = new Dictionary<(int, int), int>();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                foreach (var (innerKey, value) in cache.GetMany(outerKey, Enumerable.Range(0, 100).ToArray()))
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
        public void DontGetFromLocalCacheWhen_DontGetFromDistributedCacheWhen_WorksAsExpected(
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
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.DontGetFromLocalCacheWhen(outerKey => outerKey % 2 == 0);
           
            if (flag2)
                config.DontGetFromLocalCacheWhen((outerKey, innerKey) => innerKey % 3 == 0);
            
            if (flag3)
                config.DontGetFromDistributedCacheWhen(outerKey => outerKey % 5 == 0);
            
            if (flag4)
                config.DontGetFromDistributedCacheWhen((outerKey, innerKey) => innerKey % 7 == 0);
            
            var cachedFunction = config.Build();

            // Fill local cache with 2's and distributed cache with 3's
            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                localCache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2).ToArray(), TimeSpan.FromSeconds(1));
                distributedCache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 3).ToArray(), TimeSpan.FromSeconds(1));
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
        public void DontStoreInLocalCacheWhen_DontStoreInDistributedCacheWhen_WorksAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4, bool flag5, bool flag6)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var localCache = new MockLocalCache<int, int, int>();
            var distributedCache = new MockDistributedCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(localCache)
                .WithDistributedCache(distributedCache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));
            
            if (flag1)
                config.DontStoreInLocalCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontStoreInLocalCacheWhen((outerKey, innerKey, _) => innerKey % 3 == 0);
           
            if (flag3)
                config.DontStoreInLocalCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            if (flag4)
                config.DontStoreInDistributedCacheWhen(outerKey => outerKey % 7 == 0);

            if (flag5)
                config.DontStoreInDistributedCacheWhen((outerKey, innerKey, _) => innerKey % 11 == 0);

            if (flag6)
                config.DontStoreInDistributedCacheWhen((outerKey, innerKey, value) => innerKey % 13 == 0);
            
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
                foreach (var (innerKey, value) in localCache.GetMany(outerKey, Enumerable.Range(0, 100).ToArray()))
                    valuesInLocalCache[(outerKey, innerKey)] = value;
                
                foreach (var (innerKey, value) in distributedCache.GetMany(outerKey, Enumerable.Range(0, 100).ToArray()).Result)
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
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void DontGetFromLocalCacheWhen_ForSingleTierCache_ForSingleTierCache(bool flag1, bool flag2)
        {
            // Func always returns 1
            // Cache always returns 2
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromLocalCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontGetFromLocalCacheWhen((outerKey, innerKey) => innerKey % 3 == 0);
            
            var cachedFunction = config.Build();

            // Fill cache with 2's
            for (var outerKey = 0; outerKey < 10; outerKey++)
                cache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2).ToArray(), TimeSpan.FromSeconds(1));

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
        public void DontStoreInLocalCacheWhen_ForSingleTierCache_ForSingleTierCache(
            bool flag1, bool flag2, bool flag3)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (flag1)
                config.DontStoreInLocalCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontStoreInLocalCacheWhen((outerKey, innerKey, _) => innerKey % 3 == 0);
            
            if (flag3)
                config.DontStoreInLocalCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10));
            }
            
            var valuesInCache = new Dictionary<(int, int), int>();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                foreach (var (innerKey, value) in cache.GetMany(outerKey, Enumerable.Range(0, 100).ToArray()))
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
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void DontGetFromDistributedCacheWhen_ForSingleTierCache_WorksAsExpected(bool flag1, bool flag2)
        {
            // Func always returns 1
            // Cache always returns 2
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockDistributedCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithDistributedCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (flag1)
                config.DontGetFromDistributedCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontGetFromDistributedCacheWhen((outerKey, innerKey) => innerKey % 3 == 0);
            
            var cachedFunction = config.Build();

            // Fill cache with 2's
            for (var outerKey = 0; outerKey < 10; outerKey++)
                cache.SetMany(outerKey, Enumerable.Range(0, 100).ToDictionary(j => j, j => 2).ToArray(), TimeSpan.FromSeconds(1)).Wait();

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
        public void DontStoreInDistributedCache_ForSingleTierCache_WorksAsExpected(
            bool flag1, bool flag2, bool flag3)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => 1);
            };

            var cache = new MockDistributedCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithDistributedCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(100));

            if (flag1)
                config.DontStoreInDistributedCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontStoreInDistributedCacheWhen((outerKey, innerKey, _) => innerKey % 3 == 0);
            
            if (flag3)
                config.DontStoreInDistributedCacheWhen((outerKey, innerKey, value) => innerKey % 5 == 0);
            
            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            for (var innerKeyBatch = 0; innerKeyBatch < 10; innerKeyBatch++)
            {
                cachedFunction(outerKey, Enumerable.Range(10 * innerKeyBatch, 10));
            }
            
            var valuesInCache = new Dictionary<(int, int), int>();

            for (var outerKey = 0; outerKey < 10; outerKey++)
            {
                foreach (var (innerKey, value) in cache.GetMany(outerKey, Enumerable.Range(0, 100).ToArray()).Result)
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
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void WithTimeToLiveFactory_KeysPassedIntoFunctionAreOnlyThoseThatWillBeStoredInCache(bool flag1, bool flag2)
        {
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var cache = new MockLocalCache<int, int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(cache)
                .DontGetFromCacheWhen(x => true)
                .WithTimeToLiveFactory((outerKey, innerKeys) => ValidateTimeToLiveFactoryKeys(outerKey, innerKeys.ToArray()));

            if (flag1)
                config.DontStoreInCacheWhen(outerKey => outerKey % 2 == 0);

            if (flag2)
                config.DontStoreInCacheWhen((outerKey, innerKey, _) => innerKey % 2 == 0);

            var cachedFunction = config.Build();

            for (var outerKey = 0; outerKey < 10; outerKey++)
                cachedFunction(outerKey, Enumerable.Range(0, 100));

            TimeSpan ValidateTimeToLiveFactoryKeys(int outerKey, IReadOnlyCollection<int> innerKeys)
            {
                if (flag1)
                    outerKey.Should().Match(k => k % 2 > 0);

                var expectedInnerKeys = flag2
                    ? Enumerable.Range(0, 50).Select(i => (2 * i) + 1)
                    : Enumerable.Range(0, 100);

                innerKeys.Should().BeEquivalentTo(expectedInnerKeys);
                
                return TimeSpan.FromSeconds(1);
            }
        }

        [Theory]
        [InlineData(BatchBehaviour.FillBatchesEvenly, false, 8, 8, 9)]
        [InlineData(BatchBehaviour.FillEachBatchBeforeStartingNext, false, 10, 10, 5)]
        [InlineData(BatchBehaviour.FillBatchesEvenly, true, 8, 8, 9)]
        [InlineData(BatchBehaviour.FillEachBatchBeforeStartingNext, true, 10, 10, 5)]
        public void WithBatchedFetches_BatchesFetchesCorrectly(BatchBehaviour batchBehaviour, bool useFactoryFunction, params int[] expectedBatchSizes)
        {
            var batchSizes = new List<int>();
            
            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (outerKey, innerKeys) =>
            {
                batchSizes.Add(innerKeys.Count());
                return innerKeys.ToDictionary(k => k, k => outerKey + k);
            };

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithLocalCache(new MockLocalCache<int, int, int>())
                .WithTimeToLive(TimeSpan.FromSeconds(1));

            if (useFactoryFunction)
                config.WithBatchedFetches((_, __) => 10, batchBehaviour);
            else
                config.WithBatchedFetches(10, batchBehaviour);
                    
            var cachedFunction = config.Build();

            cachedFunction(0, Enumerable.Range(0, 25)).Keys.Should().BeEquivalentTo(Enumerable.Range(0, 25));

            batchSizes.Should().BeEquivalentTo(expectedBatchSizes);
        }
        
        [Fact]
        public void OnResult_EventsTriggeredAsExpected()
        {
            var cache = new MockLocalCache<string, int, int>();
            SuccessfulRequestEvent<string, string, int, int> lastSuccess = default;
            ExceptionEvent<string, string, int> lastException = default;

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
                .UseFirstParamAsOuterCacheKey()
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
                lastSuccess.OuterKey.Should().Be("abc");
                lastSuccess.InnerKeys.ToArray().Should().BeEquivalentTo(keys);
                lastSuccess.Values.Should().BeEquivalentTo(expectedResponse);
                lastSuccess.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastSuccess.CacheStats.CacheHits.Should().Be(wasCached ? keys.Count : 0);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Should().Be("abc");
                lastException.OuterKey.Should().Be("abc");
                lastException.InnerKeys.ToArray().Should().BeEquivalentTo(keys);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void CacheStats_PopulatedCorrectly(bool localCacheEnabled, bool distributedCacheEnabled)
        {
            CacheGetManyStats lastCacheStats = default;

            Func<int, IEnumerable<int>, Dictionary<int, int>> originalFunction = (_, keys) =>
            {
                return keys.ToDictionary(k => k);
            };

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithEnumerableKeys<int, IEnumerable<int>, Dictionary<int, int>, int, int>()
                .UseFirstParamAsOuterCacheKey()
                .WithTimeToLive(TimeSpan.FromMinutes(1))
                .OnResult(r => lastCacheStats = r.CacheStats)
                .DontGetFromCacheWhen((_, k) => k == 1)
                .DontGetFromLocalCacheWhen((_, k) => k == 2)
                .DontGetFromDistributedCacheWhen((_, k) => k == 3)
                .DontStoreInLocalCacheWhen((_, k, v) => true)
                .DontStoreInDistributedCacheWhen((_, k, v) => true);

            if (localCacheEnabled)
            {
                var localCache = new MockLocalCache<int, int, int>();
                localCache.Set(0, 4, 4, TimeSpan.FromMinutes(1));

                config.WithLocalCache(localCache);
            }

            if (distributedCacheEnabled)
            {
                var distributedCache = new MockDistributedCache<int, int, int>();
                distributedCache.Set(0, 4, 4, TimeSpan.FromMinutes(1));
                distributedCache.Set(0, 5, 5, TimeSpan.FromMinutes(1));

                config.WithDistributedCache(distributedCache);
            }

            var cachedFunction = config.Build();

            var cacheEnabled = localCacheEnabled || distributedCacheEnabled;

            for (var i = 1; i <= 5; i++)
            {
                for (var j = 1; j < 5; j++)
                {
                    var keys = Enumerable.Range(i, j).ToList();
                    
                    cachedFunction(0, keys).Should().ContainKeys(keys);
                    
                    VerifyCacheStats(keys);
                }
            }

            void VerifyCacheStats(List<int> keys)
            {
                var localCacheKeysRequested = keys.Where(k => localCacheEnabled && k != 1 && k != 2).ToList();
                var localCacheHits = localCacheKeysRequested.Where(k => k == 4).ToList();

                var distributedCacheKeysRequested = keys
                    .Except(localCacheHits)
                    .Where(k => distributedCacheEnabled && k != 1 && k != 3)
                    .ToList();
                
                var distributedCacheHits = distributedCacheKeysRequested.Where(k => k == 4 || k == 5).ToList();

                var cacheKeysRequested = keys.Where(k => cacheEnabled && k != 1).ToList();
                var cacheHits = localCacheHits.Concat(distributedCacheHits).ToList();
                
                lastCacheStats.CacheEnabled.Should().Be(cacheEnabled);
                lastCacheStats.CacheKeysRequested.Should().Be(cacheKeysRequested.Count);
                lastCacheStats.CacheKeysSkipped.Should().Be(cacheEnabled && keys.Contains(1) ? 1 : 0);
                lastCacheStats.CacheHits.Should().Be(cacheHits.Count);
                lastCacheStats.CacheMisses.Should().Be(cacheKeysRequested.Count - cacheHits.Count);
                
                lastCacheStats.LocalCacheEnabled.Should().Be(localCacheEnabled);
                lastCacheStats.LocalCacheKeysRequested.Should().Be(localCacheKeysRequested.Count);
                lastCacheStats.LocalCacheKeysSkipped.Should().Be(localCacheEnabled && keys.Contains(2) ? 1 : 0);
                lastCacheStats.LocalCacheHits.Should().Be(localCacheHits.Count);
                lastCacheStats.LocalCacheMisses.Should().Be(localCacheKeysRequested.Count - localCacheHits.Count);
                
                lastCacheStats.DistributedCacheEnabled.Should().Be(distributedCacheEnabled);
                lastCacheStats.DistributedCacheKeysRequested.Should().Be(distributedCacheKeysRequested.Count);
                lastCacheStats.DistributedCacheKeysSkipped.Should().Be(distributedCacheEnabled && keys.Contains(3) ? 1 : 0);
                lastCacheStats.DistributedCacheHits.Should().Be(distributedCacheHits.Count);
                lastCacheStats.DistributedCacheMisses.Should().Be(distributedCacheKeysRequested.Count - distributedCacheHits.Count);
            }
        }
    }
}