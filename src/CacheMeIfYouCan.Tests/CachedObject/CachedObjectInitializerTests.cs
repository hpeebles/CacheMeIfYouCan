using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class CachedObjectInitializerTests
    {
        [Fact]
        public async Task MultipleCanBeInitializedViaInitializeAll()
        {
            var ticks = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow.Ticks)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();
            
            var ticksDouble = CachedObjectFactory
                .ConfigureFor(() => (double)DateTime.UtcNow.Ticks)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            var initializeResults = await CachedObjectInitializer.InitializeAll();
            
            Assert.True(initializeResults.Success);
            
            var ticksValue = ticks.Value;
            var dateValue = date.Value;
            var ticksDoubleValue = ticksDouble.Value;

            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.NotEqual(ticksValue, ticks.Value);
            Assert.NotEqual(dateValue, date.Value);
            Assert.NotEqual(ticksDoubleValue, ticksDouble.Value);
        }
        
        [Fact]
        public async Task CanInitializeMultipleOfTheSameType()
        {
            for (var i = 0; i < 10; i++)
            {
                CachedObjectFactory
                    .ConfigureFor(Guid.NewGuid)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            var initializeResults = await CachedObjectInitializer.Initialize<Guid>();
            
            Assert.True(initializeResults.Success);
            Assert.Equal(10, initializeResults.Results.Count);
        }
        
        [Fact]
        public async Task CallingInitializeMultipleTimesSucceeds()
        {
            CachedObjectFactory
                .ConfigureFor(() => new Dummy1())
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            for (var i = 0; i < 10; i++)
            {
                var initializeResults = await CachedObjectInitializer.Initialize<Dummy1>();

                Assert.True(initializeResults.Success);
                Assert.Single(initializeResults.Results);
            }
        }

        [Fact]
        public async Task RemovedFromInitializerOnceDisposed()
        {
            var cachedFloat = CachedObjectFactory
                .ConfigureFor(() => new Dummy2())
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            var initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            Assert.True(initializeResults.Success);
            Assert.Single(initializeResults.Results);

            cachedFloat.Dispose();

            initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            Assert.False(initializeResults.Success);
            Assert.Empty(initializeResults.Results);
        }

        [Fact]
        public async Task InitializeDurationIsAccurate()
        {
            CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return new Dummy3();
                })
                .WithRefreshInterval(TimeSpan.FromMinutes(1))
                .Build();

            var initializeResult = await CachedObjectInitializer.Initialize<Dummy3>();
            
            Assert.True(initializeResult.Success);
            Assert.Single(initializeResult.Results);
            Assert.InRange(initializeResult.Results[0].Duration, TimeSpan.FromSeconds(0.9), TimeSpan.FromSeconds(1.1));
        }

        private class Dummy1
        {}
        
        private class Dummy2
        {}
        
        private class Dummy3
        {}
    }
}