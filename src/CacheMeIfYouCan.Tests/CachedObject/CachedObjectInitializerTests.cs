using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class CachedObjectInitializerTests : CachedObjectTestBase
    {
        [Fact]
        public async Task MultipleCanBeInitializedViaInitializeAll()
        {
            ICachedObject<long> ticks;
            ICachedObject<DateTime> date;
            ICachedObject<double> ticksDouble;
            using (EnterSetup(false))
            {
                ticks = CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return DateTime.UtcNow.Ticks;
                    })
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();

                date = CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return DateTime.UtcNow;
                    })
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();

                ticksDouble = CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return (double) DateTime.UtcNow.Ticks;
                    })
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            var initializeResults = await CachedObjectInitializer.InitializeAll();
            
            Assert.True(initializeResults.Success);

            var timer = Stopwatch.StartNew();
            
            var ticksValue = ticks.Value;
            var dateValue = date.Value;
            var ticksDoubleValue = ticksDouble.Value;

            Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(100));
        }
        
        [Fact]
        public async Task CanInitializeMultipleOfTheSameType()
        {
            using (EnterSetup(false))
            {
                for (var i = 0; i < 10; i++)
                {
                    CachedObjectFactory
                        .ConfigureFor(Guid.NewGuid)
                        .WithRefreshInterval(TimeSpan.FromSeconds(1))
                        .Build();
                }
            }

            var initializeResults = await CachedObjectInitializer.Initialize<Guid>();
            
            Assert.True(initializeResults.Success);
            Assert.Equal(10, initializeResults.Results.Count);
        }
        
        [Fact]
        public async Task CallingInitializeMultipleTimesSucceeds()
        {
            using (EnterSetup(false))
            {
                CachedObjectFactory
                    .ConfigureFor(() => new Dummy1())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

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
            ICachedObject<Dummy2> cachedObj;
            using (EnterSetup(false))
            {
                cachedObj = CachedObjectFactory
                    .ConfigureFor(() => new Dummy2())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            var initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            Assert.True(initializeResults.Success);
            Assert.Single(initializeResults.Results);

            cachedObj.Dispose();

            initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            Assert.False(initializeResults.Success);
            Assert.Empty(initializeResults.Results);
        }

        [Fact]
        public async Task InitializeDurationIsAccurate()
        {
            using (EnterSetup(false))
            {
                CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return new Dummy3();
                    })
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            var timer = Stopwatch.StartNew();
            
            var initializeResult = await CachedObjectInitializer.Initialize<Dummy3>();
            
            timer.Stop();
            
            Assert.True(initializeResult.Success);
            Assert.Single(initializeResult.Results);
            Assert.InRange(initializeResult.Results[0].Duration, TimeSpan.FromSeconds(0.9), timer.Elapsed);
        }

        private class Dummy1
        {}
        
        private class Dummy2
        {}
        
        private class Dummy3
        {}
    }
}