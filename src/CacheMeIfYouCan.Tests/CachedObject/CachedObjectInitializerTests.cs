using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class CachedObjectInitializerTests
    {
        private readonly CachedObjectSetupLock _setupLock;

        public CachedObjectInitializerTests(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task MultipleCanBeInitializedViaInitializeAll()
        {
            ICachedObject<long> ticks;
            ICachedObject<DateTime> date;
            ICachedObject<double> ticksDouble;
            using (_setupLock.Enter())
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
            
            initializeResults.Success.Should().BeTrue();

            var timer = Stopwatch.StartNew();
            
            var ticksValue = ticks.Value;
            var dateValue = date.Value;
            var ticksDoubleValue = ticksDouble.Value;

            timer.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
        }
        
        [Fact]
        public async Task CanInitializeMultipleOfTheSameType()
        {
            using (_setupLock.Enter())
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
            
            initializeResults.Success.Should().BeTrue();
            initializeResults.Results.Count.Should().Be(10);
        }
        
        [Fact]
        public async Task CallingInitializeMultipleTimesSucceeds()
        {
            using (_setupLock.Enter())
            {
                CachedObjectFactory
                    .ConfigureFor(() => new Dummy1())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var initializeResults = await CachedObjectInitializer.Initialize<Dummy1>();

                initializeResults.Success.Should().BeTrue();
                initializeResults.Results.Should().ContainSingle();
            }
        }

        [Fact]
        public async Task RemovedFromInitializerOnceDisposed()
        {
            ICachedObject<Dummy2> cachedObj;
            using (_setupLock.Enter())
            {
                cachedObj = CachedObjectFactory
                    .ConfigureFor(() => new Dummy2())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            var initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            initializeResults.Success.Should().BeTrue();
            initializeResults.Results.Should().ContainSingle();

            cachedObj.Dispose();

            initializeResults = await CachedObjectInitializer.Initialize<Dummy2>();

            initializeResults.Success.Should().BeFalse();
            initializeResults.Results.Should().BeEmpty();
        }

        [Fact]
        public async Task InitializeDurationIsAccurate()
        {
            using (_setupLock.Enter())
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
            
            initializeResult.Success.Should().BeTrue();
            initializeResult.Results.Should().ContainSingle();
            initializeResult.Results[0].Duration
                .Should().BeGreaterThan(TimeSpan.FromSeconds(0.9))
                .And.BeLessThan(timer.Elapsed);
        }
        
        [Fact]
        public async Task NameIsSetOnInitializationResult()
        {
            var name = Guid.NewGuid().ToString();
            
            using (_setupLock.Enter())
            {
                CachedObjectFactory
                    .ConfigureFor(() => new Dummy4())
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Named(name)
                    .Build();
            }

            var initializeResult = await CachedObjectInitializer.Initialize<Dummy4>();
            
            initializeResult.Success.Should().BeTrue();
            initializeResult.Results.Should().ContainSingle();
            initializeResult.Results[0].Name.Should().Be(name);
        }

        private class Dummy1
        {}
        
        private class Dummy2
        {}
        
        private class Dummy3
        {}
        
        private class Dummy4
        {}
    }
}