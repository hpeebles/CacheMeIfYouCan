using System;
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
                await Task.Delay(delay);
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
                            var value = await cachedFunction(i);
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
                await Task.Delay(delay, cancellationToken);
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
    }
}