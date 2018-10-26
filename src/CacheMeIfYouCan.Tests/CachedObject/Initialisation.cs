using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class Initialisation
    {
        [Fact]
        public void NotInitialisedThrowsException()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Ticks)
                .RefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            Assert.Throws<Exception>(() => ticks.Value);
        }
        
        [Fact]
        public async Task CanBeInitialisedDirectly()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Ticks)
                .RefreshInterval(TimeSpan.FromSeconds(1))
                .Build(false);

            await ticks.Init();
            
            Assert.NotEqual(0, ticks.Value);
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