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
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With1Param_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, CancellationToken, Task<int>> originalFunction = (p, cancellationToken) => Task.FromResult(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, CancellationToken.None).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "async":
                {
                    Func<int, Task<int>> originalFunction = Task.FromResult;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, CancellationToken.None).Should().Be(1);
                    break;
                }
                case "sync":
                {
                    Func<int, int> originalFunction = p => p;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1).Should().Be(1);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, CancellationToken, ValueTask<int>> originalFunction = (p, cancellationToken) => new ValueTask<int>(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, CancellationToken.None).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
                case "valuetask":
                {
                    Func<int, ValueTask<int>> originalFunction = p => new ValueTask<int>(p);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1).ConfigureAwait(false)).Should().Be(1);
                    break;
                }
            }

            cache.TryGet("1", out var value).Should().BeTrue();
            value.Should().Be(1);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With2Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, cancellationToken) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, CancellationToken.None).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "async":
                {
                    Func<int, int, Task<int>> originalFunction = (p1, p2) => Task.FromResult(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, int> originalFunction = (p1, p2, cancellationToken) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, CancellationToken.None).Should().Be(3);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int> originalFunction = (p1, p2) => p1 + p2;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2).Should().Be(3);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, cancellationToken) => new ValueTask<int>(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, CancellationToken.None).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, ValueTask<int>> originalFunction = (p1, p2) => new ValueTask<int>(p1 + p2);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2).ConfigureAwait(false)).Should().Be(3);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(3);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With3Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, cancellationToken) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, CancellationToken.None).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, Task<int>> originalFunction = (p1, p2, p3) => Task.FromResult(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, cancellationToken) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, CancellationToken.None).Should().Be(6);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int> originalFunction = (p1, p2, p3) => p1 + p2 + p3;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3).Should().Be(6);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, cancellationToken) => new ValueTask<int>(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, CancellationToken.None).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3) => new ValueTask<int>(p1 + p2 + p3);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3).ConfigureAwait(false)).Should().Be(6);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(6);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With4Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, CancellationToken.None).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4) => Task.FromResult(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, cancellationToken) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, CancellationToken.None).Should().Be(10);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int> originalFunction = (p1, p2, p3, p4) => p1 + p2 + p3 + p4;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4).Should().Be(10);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, CancellationToken.None).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4) => new ValueTask<int>(p1 + p2 + p3 + p4);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4).ConfigureAwait(false)).Should().Be(10);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(10);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With5Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5) => Task.FromResult(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).Should().Be(15);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5).Should().Be(15);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5).ConfigureAwait(false)).Should().Be(15);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(15);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With6Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).Should().Be(21);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6).Should().Be(21);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6).ConfigureAwait(false)).Should().Be(21);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(21);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With7Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).Should().Be(28);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7).Should().Be(28);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7).ConfigureAwait(false)).Should().Be(28);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(28);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task With8Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();

            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).ConfigureAwait(false)).Should().Be(36);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => Task.FromResult(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    (await cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).ConfigureAwait(false)).Should().Be(36);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Should().Be(36);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Should().Be(36);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Result.Should().Be(36);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) => new ValueTask<int>(p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8);
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .Build();

                    cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Result.Should().Be(36);
                    break;
                }
            }

            cache.TryGet(1, out var value).Should().BeTrue();
            value.Should().Be(36);
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_WhenKeyIsParam_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<int, int>();
            CachedFunctionWithSingleKeyResult_Success<int, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_Exception<int> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, CancellationToken, Task<int>> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, Task<int>> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return -p;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, CancellationToken, ValueTask<int>> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, ValueTask<int>> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Key.Should().Be(1);
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Key.Should().Be(1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With1Param_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_1Param_Success<int, string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_1Param_Exception<int, string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, CancellationToken, Task<int>> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, Task<int>> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return -p;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, CancellationToken, ValueTask<int>> originalFunction = (p, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, ValueTask<int>> originalFunction = p =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey(p => p.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameter.Should().Be(1);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameter.Should().Be(1);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With2Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, Task<int>> originalFunction = (p1, p2) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, int> originalFunction = (p1, p2, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int> originalFunction = (p1, p2) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, ValueTask<int>> originalFunction = (p1, p2) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With3Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, Task<int>> originalFunction = (p1, p2, p3) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int> originalFunction = (p1, p2, p3) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With4Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int> originalFunction = (p1, p2, p3, p4) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Parameters.Item4.Should().Be(4);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Parameters.Item4.Should().Be(4);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With5Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Parameters.Item4.Should().Be(4);
                lastSuccess.Parameters.Item5.Should().Be(5);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Parameters.Item4.Should().Be(4);
                lastException.Parameters.Item5.Should().Be(5);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With6Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int, int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int, int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Parameters.Item4.Should().Be(4);
                lastSuccess.Parameters.Item5.Should().Be(5);
                lastSuccess.Parameters.Item6.Should().Be(6);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Parameters.Item4.Should().Be(4);
                lastException.Parameters.Item5.Should().Be(5);
                lastException.Parameters.Item6.Should().Be(6);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With7Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int, int, int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int, int, int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Parameters.Item4.Should().Be(4);
                lastSuccess.Parameters.Item5.Should().Be(5);
                lastSuccess.Parameters.Item6.Should().Be(6);
                lastSuccess.Parameters.Item7.Should().Be(7);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Parameters.Item4.Should().Be(4);
                lastException.Parameters.Item5.Should().Be(5);
                lastException.Parameters.Item6.Should().Be(6);
                lastException.Parameters.Item7.Should().Be(7);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
        
        [Theory]
        [InlineData("async", true)]
        [InlineData("async", false)]
        [InlineData("sync", true)]
        [InlineData("sync", false)]
        [InlineData("valuetask", true)]
        [InlineData("valuetask", false)]
        public async Task OnResult_With8Params_WorksAsExpected(string functionType, bool hasCancellationToken)
        {
            var cache = new MockLocalCache<string, int>();
            CachedFunctionWithSingleKeyResult_MultiParam_Success<(int, int, int, int, int, int, int, int), string, int> lastSuccess = default;
            CachedFunctionWithSingleKeyResult_MultiParam_Exception<(int, int, int, int, int, int, int, int), string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            switch (functionType)
            {
                case "async" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "async":
                {
                    Func<int, int, int, int, int, int, int, int, Task<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) =>
                    {
                        ThrowIfFirst();
                        return Task.FromResult(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<Task<int>> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8);
                    await func.Should().ThrowAsync<Exception>();
                    CheckException();
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(false);
                    (await func().ConfigureAwait(false)).Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "sync":
                {
                    Func<int, int, int, int, int, int, int, int, int> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) =>
                    {
                        ThrowIfFirst();
                        return -p1;
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8);
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask" when hasCancellationToken:
                {
                    Func<int, int, int, int, int, int, int, int, CancellationToken, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8, cancellationToken) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8, CancellationToken.None).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
                case "valuetask":
                {
                    Func<int, int, int, int, int, int, int, int, ValueTask<int>> originalFunction = (p1, p2, p3, p4, p5, p6, p7, p8) =>
                    {
                        ThrowIfFirst();
                        return new ValueTask<int>(-p1);
                    };
                    
                    var cachedFunction = CachedFunctionFactory
                        .ConfigureFor(originalFunction)
                        .WithCacheKey((p1, p2, p3, p4, p5, p6, p7, p8) => p1.ToString())
                        .WithLocalCache(cache)
                        .WithTimeToLive(TimeSpan.FromSeconds(1))
                        .OnResult(r => lastSuccess = r, ex => lastException = ex)
                        .Build();

                    Func<int> func = () => cachedFunction(1, 2, 3, 4, 5, 6, 7, 8).Result;
                    func.Should().Throw<Exception>();
                    CheckException();
                    func().Should().Be(-1);
                    CheckSuccess(false);
                    func().Should().Be(-1);
                    CheckSuccess(true);
                    break;
                }
            }

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
                lastSuccess.Parameters.Item1.Should().Be(1);
                lastSuccess.Parameters.Item2.Should().Be(2);
                lastSuccess.Parameters.Item3.Should().Be(3);
                lastSuccess.Parameters.Item4.Should().Be(4);
                lastSuccess.Parameters.Item5.Should().Be(5);
                lastSuccess.Parameters.Item6.Should().Be(6);
                lastSuccess.Parameters.Item7.Should().Be(7);
                lastSuccess.Parameters.Item8.Should().Be(8);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastSuccess.WasCached.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Item1.Should().Be(1);
                lastException.Parameters.Item2.Should().Be(2);
                lastException.Parameters.Item3.Should().Be(3);
                lastException.Parameters.Item4.Should().Be(4);
                lastException.Parameters.Item5.Should().Be(5);
                lastException.Parameters.Item6.Should().Be(6);
                lastException.Parameters.Item7.Should().Be(7);
                lastException.Parameters.Item8.Should().Be(8);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero);
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }
    }
}