using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;
using CacheMeIfYouCan.Tests.Generators;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public partial class CachedFunctionWithSingleKeyTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Concurrent_CorrectValueIsReturned(bool useMemoryCache)
        {
            var delay = TimeSpan.FromMilliseconds(100);
            
            Func<int, Task<int>> originalFunction = async i =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                return i;
            };

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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
                            var value = await cachedFunction(i).ConfigureAwait(false);
                            value.Should().Be(i);
                        }
                    })
                    .ToArray();

                Task.WaitAll(tasks);
            }
        }

        [Fact]
        public async Task WithCancellationToken_CancellationPropagatesToUnderlyingFunction()
        {
            var wasCancelled = false;
            
            Func<int, CancellationToken, Task<int>> originalFunction = async (key, cancellationToken) =>
            {
                cancellationToken.Register(() => wasCancelled = true);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                return key;
            };

            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(new MemoryCache<int, int>(i => i.ToString()))
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .Build();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            
            Func<Task<int>> func = () => cachedFunction(1, cancellationTokenSource.Token);

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
        public void DontGetFromCacheWhen_DontStoreInCacheWhen_WorksForAllCombinations(bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
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
        public void DontGetFromOrStoreInLocalCacheWhen_DontGetFromOrStoreInDistributedCacheWhen_WorksAsExpected(
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
                var skipLocalSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                var skipDistributedGet = flag5 && i % 11 == 0 || flag7 && i % 17 == 0;
                var skipDistributedSet = flag6 && i % 13 == 0 || flag8 && i % 19 == 0;
                
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
        public void DontGetFromOrStoreInLocalCacheWhen_ForSingleTierCache_WorksAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockLocalCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
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
        public void DontGetFromOrStoreInDistributedCacheWhen_ForSingleTierCache_WorksAsExpected(
            bool flag1, bool flag2, bool flag3, bool flag4)
        {
            Func<int, int> originalFunction = key => key;

            var cache = new MockDistributedCache<int, int>();

            var config = CachedFunctionFactory
                .ConfigureFor(originalFunction)
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

            var previousTryGetExecutionCount = 0;
            var previousSetExecutionCount = 0;
            for (var i = 0; i < 100; i++)
            {
                cachedFunction(i);

                var currentTryGetExecutionCount = cache.TryGetExecutionCount;
                var currentSetExecutionCount = cache.SetExecutionCount;

                var skipGet = flag1 && i % 2 == 0 || flag3 && i % 5 == 0;
                var skipSet = flag2 && i % 3 == 0 || flag4 && i % 7 == 0;
                
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
        public void WithTimeToLiveFactory_NotCalledIfKeyWillNotBeStoredInCache()
        {
            Func<int, int> originalFunction = key => key;

            var timeToLiveFactoryExecutionCount = 0;
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithLocalCache(new MockLocalCache<int, int>())
                .DontStoreInCacheWhen((k, _) => k % 2 == 0)
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

        [Fact]
        public void OnResult_EventsAreTriggeredAsExpected()
        {
            var cache = new MockLocalCache<string, int>();
            SuccessfulRequestEvent<int, string, int> lastSuccess = default;
            ExceptionEvent<int, string> lastException = default;

            var first = true;
            var exceptionMessage = Guid.NewGuid().ToString();
            
            Func<int, CancellationToken, int> originalFunction = (p, cancellationToken) =>
            {
                ThrowIfFirst();
                return -p;
            };
            
            var cachedFunction = CachedFunctionFactory
                .ConfigureFor(originalFunction)
                .WithCacheKey(x => x.ToString())
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
                lastSuccess.Parameters.Should().Be(1);
                lastSuccess.Key.Should().Be("1");
                lastSuccess.Value.Should().Be(-1);
                lastSuccess.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastSuccess.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastSuccess.CacheStats.CacheHit.Should().Be(wasCached);
            }

            void CheckException()
            {
                var now = DateTime.UtcNow;
                lastException.Parameters.Should().Be(1);
                lastException.Key.Should().Be("1");
                lastException.Start.Should().BeWithin(TimeSpan.FromMilliseconds(100)).Before(now);
                lastException.Duration.Should().BePositive().And.BeCloseTo(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
                lastException.Exception.Message.Should().Be(exceptionMessage);
            }
        }

        [Theory]
        [MemberData(nameof(BoolGenerator.GetAllCombinations), 2, MemberType = typeof(BoolGenerator))]
        public void CacheStats_PopulatedCorrectly(bool localCacheEnabled, bool distributedCacheEnabled)
        {
            CacheGetStats lastCacheStats = default;

            Func<int, int> originalFunction = key => key;

            var config = CachedFunctionFactory.ConfigureFor(originalFunction)
                .WithTimeToLive(TimeSpan.FromMinutes(1))
                .OnResult(r => lastCacheStats = r.CacheStats)
                .DontGetFromCacheWhen(k => k == 1)
                .DontGetFromLocalCacheWhen(k => k == 2)
                .DontGetFromDistributedCacheWhen(k => k == 3);

            if (localCacheEnabled)
            {
                var localCache = new MockLocalCache<int, int>();
                localCache.Set(4, 4, TimeSpan.FromMinutes(1));

                config.WithLocalCache(localCache);
            }

            if (distributedCacheEnabled)
            {
                var distributedCache = new MockDistributedCache<int, int>();
                distributedCache.Set(4, 4, TimeSpan.FromMinutes(1));
                distributedCache.Set(5, 5, TimeSpan.FromMinutes(1));

                config.WithDistributedCache(distributedCache);
            }

            var cachedFunction = config.Build();

            var cacheEnabled = localCacheEnabled || distributedCacheEnabled;

            for (var i = 1; i <= 5; i++)
            {
                cachedFunction(i).Should().Be(i);

                VerifyCacheStats(i);
            }

            void VerifyCacheStats(int key)
            {
                var localCacheKeyRequested = localCacheEnabled && key != 1 && key != 2;
                var localCacheHit = localCacheEnabled && key == 4;

                var distributedCacheKeyRequested = distributedCacheEnabled && !localCacheHit && key != 1 && key != 3;
                var distributedCacheHit = distributedCacheKeyRequested && (key == 4 || key == 5);

                var cacheKeyRequested = localCacheKeyRequested || distributedCacheKeyRequested;
                var cacheHit = localCacheHit || distributedCacheHit;
                
                lastCacheStats.CacheEnabled.Should().Be(cacheEnabled);
                lastCacheStats.CacheKeyRequested.Should().Be(cacheKeyRequested);
                lastCacheStats.CacheSkipped.Should().Be(cacheEnabled && !cacheKeyRequested);
                lastCacheStats.CacheHit.Should().Be(cacheHit);
                lastCacheStats.CacheMiss.Should().Be(cacheKeyRequested && !cacheHit);
                
                lastCacheStats.LocalCacheEnabled.Should().Be(localCacheEnabled);
                lastCacheStats.LocalCacheKeyRequested.Should().Be(localCacheKeyRequested);
                lastCacheStats.LocalCacheSkipped.Should().Be(localCacheEnabled && !localCacheKeyRequested);
                lastCacheStats.LocalCacheHit.Should().Be(localCacheHit);
                lastCacheStats.LocalCacheMiss.Should().Be(localCacheKeyRequested && !localCacheHit);
                
                lastCacheStats.DistributedCacheEnabled.Should().Be(distributedCacheEnabled);
                lastCacheStats.DistributedCacheKeyRequested.Should().Be(distributedCacheKeyRequested);
                lastCacheStats.DistributedCacheSkipped.Should().Be(distributedCacheEnabled && !localCacheHit && !distributedCacheKeyRequested);
                lastCacheStats.DistributedCacheHit.Should().Be(distributedCacheHit);
                lastCacheStats.DistributedCacheMiss.Should().Be(distributedCacheKeyRequested && !distributedCacheHit);
            }
        }
    }
}