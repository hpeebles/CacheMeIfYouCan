using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
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
                .RefreshInterval(TimeSpan.FromSeconds(1))
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
                .RefreshInterval(TimeSpan.FromSeconds(1))
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
                .RefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var millis = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Millisecond)
                .RefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var ticksDouble = CachedObjectFactory
                .ConfigureFor(() => (double)DateTime.UtcNow.Ticks)
                .RefreshInterval(TimeSpan.FromSeconds(1))
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
    }
}