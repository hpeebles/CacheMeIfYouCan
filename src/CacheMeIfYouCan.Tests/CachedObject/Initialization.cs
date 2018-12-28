using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class Initialization
    {
        [Fact]
        public void NotInitializedWillInitializeOnFirstCall()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return DateTime.UtcNow.Ticks;
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed >= TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public async Task CanBeInitializedDirectly()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return DateTime.UtcNow.Ticks;
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            await ticks.Initialize();

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(20));
        }

        [Fact]
        public void InitializationOnlyRunsOnce()
        {
            var count = 0;
            
            var ticksAsync = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Interlocked.Increment(ref count);
                    return Task.Delay(100).ContinueWith(t => DateTime.UtcNow.Ticks);
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            var values = Enumerable
                .Range(0, 1000)
                .AsParallel()
                .Select(i => ticksAsync.Value)
                .ToArray();

            Assert.Equal(1, count);
        }
    }
}