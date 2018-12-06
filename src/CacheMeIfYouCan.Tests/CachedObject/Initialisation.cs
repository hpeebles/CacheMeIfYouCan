using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class Initialisation
    {
        [Fact]
        public void NotInitialisedWillInitialiseOnFirstCall()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return DateTime.UtcNow.Ticks;
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed >= TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public async Task CanBeInitialisedDirectly()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return DateTime.UtcNow.Ticks;
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            await ticks.Init();

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(20));
        }
        
        [Fact]
        public async Task MultipleCanBeInitialisedViaCachedObjectInitialiser()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Ticks)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var millis = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Millisecond)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var ticksDouble = CachedObjectFactory
                .ConfigureFor(() => (double)DateTime.UtcNow.Ticks)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            await CachedObjectInitialiser.InitAll();
            
            var ticksValue = ticks.Value;
            var millisValue = millis.Value;
            var ticksDoubleValue = ticksDouble.Value;

            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.NotEqual(ticksValue, ticks.Value);
            Assert.NotEqual(millisValue, millis.Value);
            Assert.NotEqual(ticksDoubleValue, ticksDouble.Value);
        }

        [Fact]
        public void InitialisationOnlyRunsOnce()
        {
            var count = 0;
            
            var ticksAsync = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    Interlocked.Increment(ref count);
                    return Task.Delay(100).ContinueWith(t => DateTime.UtcNow.Ticks);
                })
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            var values = Enumerable
                .Range(0, 1000)
                .AsParallel()
                .Select(i => ticksAsync.Value)
                .ToArray();

            Assert.Equal(1, count);
        }
    }
}