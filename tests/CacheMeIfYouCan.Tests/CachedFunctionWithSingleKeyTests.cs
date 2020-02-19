using System;
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
    public class CachedFunctionWithSingleKeyTests
    {
        [Fact]
        public void Concurrent_CorrectValueIsReturned()
        {
            var delay = TimeSpan.FromMilliseconds(100);
            
            Func<int, Task<int>> originalFunction = async i =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                return i;
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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
                            var value = await cachedFunction(i).ConfigureAwait(false);
                            value.Should().Be(i);
                        }
                    })
                    .ToArray();

                Task.WaitAll(tasks);
            }
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(500, false)]
        public async Task WithCancellationToken_ThrowsIfCancelled(int cancelAfter, bool shouldThrow)
        {
            var delay = TimeSpan.FromMilliseconds(200);
            
            Func<int, CancellationToken, Task<int>> originalFunction = async (i, cancellationToken) =>
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                return i;
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(cancelAfter);
            
            Func<Task<int>> func = () => cachedFunction(1, cancellationTokenSource.Token);

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
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromMilliseconds(timeToLiveMs))
                .Build();

            cachedFunction(1);
            
            Thread.Sleep(timeToLiveMs / 2);
            
            cachedFunction(1);
            cache.HitsCount.Should().Be(1);
            
            Thread.Sleep(timeToLiveMs);

            cachedFunction(1);
            cache.HitsCount.Should().Be(1);
        }
        
        [Fact]
        public void WithTimeToLiveFactory_SetsTimeToLiveCorrectly()
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLiveFactory(key => TimeSpan.FromMilliseconds(key))
                .Build();

            foreach (var key in new[] { 250, 500, 1000 })
                CheckTimeToLiveForKey(key);

            void CheckTimeToLiveForKey(int key)
            {
                cachedFunction(key);

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
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(cache)
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .DisableCaching(disableCaching)
                .Build();

            cachedFunction(1);

            if (disableCaching)
            {
                cache.TryGetExecutionCount.Should().Be(0);
                cache.SetExecutionCount.Should().Be(0);
            }
            else
            {
                cache.TryGetExecutionCount.Should().Be(1);
                cache.SetExecutionCount.Should().Be(1);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void SkipCacheGet_SkipCacheSet_WorksForAllCombinations(bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount);
                else
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount + 1);

                if (skipSet)
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount);
                else
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount + 1);

                previousTryGetExecutionCount = currentTryGetExecutionCount;
                previousSetExecutionCount = currentSetExecutionCount;
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 8, MemberType = typeof(BoolGenerator))]
        public void SkipLocalCacheWhen_SkipDistributedCacheWhen_WorkAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4, bool flag5, bool flag6, bool flag7, bool flag8)
        {
            Func<int, int> originalFunction = key => key;

            var localCache = new MockLocalCache<int, int>();
            var distributedCache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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
            
            var previousLocalTryGetExecutionCount = 0;
            var previousLocalSetExecutionCount = 0;
            var previousDistributedTryGetExecutionCount = 0;
            var previousDistributedSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentLocalTryGetExecutionCount = localCache.TryGetExecutionCount;
                var currentLocalSetExecutionCount = localCache.SetExecutionCount;
                var currentDistributedTryGetExecutionCount = distributedCache.TryGetExecutionCount;
                var currentDistributedSetExecutionCount = distributedCache.SetExecutionCount;

                var skipLocalGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipLocalSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                var skipDistributedGet = flag5 && i % 11 == 0 || flag7 && i % 17 == 0;
                var skipDistributedSet = flag6 && i % 13 == 0 || flag7 && i % 17 == 0 || flag8 && i % 19 == 0;
                
                if (skipLocalGet)
                    currentLocalTryGetExecutionCount.Should().Be(previousLocalTryGetExecutionCount);
                else
                    currentLocalTryGetExecutionCount.Should().Be(previousLocalTryGetExecutionCount + 1);

                if (skipLocalSet)
                    currentLocalSetExecutionCount.Should().Be(previousLocalSetExecutionCount);
                else
                    currentLocalSetExecutionCount.Should().Be(previousLocalSetExecutionCount + 1);

                if (skipDistributedGet)
                    currentDistributedTryGetExecutionCount.Should().Be(previousDistributedTryGetExecutionCount);
                else
                    currentDistributedTryGetExecutionCount.Should().Be(previousDistributedTryGetExecutionCount + 1);

                if (skipDistributedSet)
                    currentDistributedSetExecutionCount.Should().Be(previousDistributedSetExecutionCount);
                else
                    currentDistributedSetExecutionCount.Should().Be(previousDistributedSetExecutionCount + 1);
                
                previousLocalTryGetExecutionCount = currentLocalTryGetExecutionCount;
                previousLocalSetExecutionCount = currentLocalSetExecutionCount;
                previousDistributedTryGetExecutionCount = currentDistributedTryGetExecutionCount;
                previousDistributedSetExecutionCount = currentDistributedSetExecutionCount;
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void SkipLocalCacheWhen_ForSingleTierCache_WorksTheSameAsUsingSkipCacheWhen(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount);
                else
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount + 1);

                if (skipSet)
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount);
                else
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount + 1);

                previousTryGetExecutionCount = currentTryGetExecutionCount;
                previousSetExecutionCount = currentSetExecutionCount;
            }
        }
        
        
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 4, MemberType = typeof(BoolGenerator))]
        public void SkipDistributedCacheWhen_ForSingleTierCache_WorksTheSameAsUsingSkipCacheWhen(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag3 && i % 5 == 0 || flag4 && i % 7 == 0;
                
                if (skipGet)
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount);
                else
                    currentTryGetExecutionCount.Should().Be(previousTryGetExecutionCount + 1);

                if (skipSet)
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount);
                else
                    currentSetExecutionCount.Should().Be(previousSetExecutionCount + 1);

                previousTryGetExecutionCount = currentTryGetExecutionCount;
                previousSetExecutionCount = currentSetExecutionCount;
            }
        }

        [Fact]
        public void WithTimeToLiveFactory_NotCalledIfKeyWillSkipCaching()
        {
            Func<int, int> originalFunction = key => key;

            var timeToLiveFactoryExecutionCount = 0;
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(new MockLocalCache<int, int>())
                .SkipCacheWhen(k => k % 2 == 0)
                .WithTimeToLiveFactory(TimeToLiveFactory)
                .Build();

            for (var i = 0; i < 100; i++)
                cachedFunction(i);

            timeToLiveFactoryExecutionCount.Should().Be(50);
            
            TimeSpan TimeToLiveFactory(int key)
            {
                Interlocked.Increment(ref timeToLiveFactoryExecutionCount);

                if (key % 2 == 0)
                    throw new Exception();
                
                return TimeSpan.FromSeconds(1);
            }
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_1Param_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, CancellationToken, Task<int>> originalFunction = (p, cancellationToken) => Task.FromResult(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, CancellationToken.None).ConfigureAwait(false)).Should().Be(1);
                }
                else
                {
                    Func<int, Task<int>> originalFunction = Task.FromResult;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1).ConfigureAwait(false)).Should().Be(1);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, CancellationToken.None).Should().Be(1);
                }
                else
                {
                    Func<int, int> originalFunction = p => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1).Should().Be(1);
                }
            }

            cache.TryGet("1", out var value).Should().BeTrue();
            value.Should().Be(1);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_2Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, cancellationToken) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, CancellationToken.None).ConfigureAwait(false)).Should().Be(3);
                }
                else
                {
                    Func<int, int, Task<int>> originalFunction = (p1, p2) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2).ConfigureAwait(false)).Should().Be(3);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, CancellationToken, int> originalFunction = (p1, p2, cancellationToken) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, CancellationToken.None).Should().Be(3);
                }
                else
                {
                    Func<int, int, int> originalFunction = (p1, p2) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2).Should().Be(3);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(3);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_3Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, cancellationToken) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, CancellationToken.None).ConfigureAwait(false)).Should().Be(6);
                }
                else
                {
                    Func<int, int, int, Task<int>> originalFunction = (p1, p2, p3) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3).ConfigureAwait(false)).Should().Be(6);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, cancellationToken) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, CancellationToken.None).Should().Be(6);
                }
                else
                {
                    Func<int, int, int, int> originalFunction = (p1, p2, p3) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3).Should().Be(6);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(6);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_4Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, CancellationToken.None).ConfigureAwait(false)).Should().Be(10);
                }
                else
                {
                    Func<int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4).ConfigureAwait(false)).Should().Be(10);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, cancellationToken) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, CancellationToken.None).Should().Be(10);
                }
                else
                {
                    Func<int, int, int, int, int> originalFunction = (p1, p2, p3, p4) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4).Should().Be(10);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(10);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_5Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).ConfigureAwait(false)).Should().Be(15);
                }
                else
                {
                    Func<int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5).ConfigureAwait(false)).Should().Be(15);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).Should().Be(15);
                }
                else
                {
                    Func<int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5).Should().Be(15);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(15);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_6Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).ConfigureAwait(false)).Should().Be(21);
                }
                else
                {
                    Func<int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6).ConfigureAwait(false)).Should().Be(21);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).Should().Be(21);
                }
                else
                {
                    Func<int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6).Should().Be(21);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(21);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_7Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).ConfigureAwait(false)).Should().Be(28);
                }
                else
                {
                    Func<int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7).ConfigureAwait(false)).Should().Be(28);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).Should().Be(28);
                }
                else
                {
                    Func<int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7).Should().Be(28);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(28);
        }
        
        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public async Task WithKeySelector_8Params_WorksAsExpected(bool isAsync, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            if (isAsync)
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).ConfigureAwait(false)).Should().Be(36);
                }
                else
                {
                    Func<int, int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).ConfigureAwait(false)).Should().Be(36);
                }
            }
            else
            {
                if (hasCancellationToken)
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Should().Be(36);
                }
                else
                {
                    Func<int, int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKeySelector((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Should().Be(36);
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(36);
        }
    }
}