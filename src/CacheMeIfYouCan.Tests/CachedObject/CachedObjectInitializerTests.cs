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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task MultipleCanBeInitializedViaInitializeAll(bool async)
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

            var initializeResults = async
                ? await CachedObjectInitializer.InitializeAllAsync()
                : CachedObjectInitializer.InitializeAll();
            
            initializeResults.Success.Should().BeTrue();
            initializeResults.Duration.Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThan(TimeSpan.FromSeconds(2));

            var timer = Stopwatch.StartNew();
            
            var ticksValue = ticks.Value;
            var dateValue = date.Value;
            var ticksDoubleValue = ticksDouble.Value;

            timer.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
            
            ticks.Dispose();
            date.Dispose();
            ticksDouble.Dispose();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CallingInitializeAllMultipleTimesSucceeds(bool async)
        {
            ICachedObject cachedObject;
            using (_setupLock.Enter())
            {
                cachedObject = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var initializeResults = async
                    ? await CachedObjectInitializer.InitializeAllAsync()
                    : CachedObjectInitializer.InitializeAll();

                initializeResults.Success.Should().BeTrue();
                initializeResults.Results.Should().ContainSingle();
            }
            
            cachedObject.Dispose();
        }

        [Fact]
        public void RemovedFromInitializerOnceDisposed()
        {
            ICachedObject cachedObject;
            using (_setupLock.Enter())
            {
                cachedObject = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            var initializeResults = CachedObjectInitializer.InitializeAll();

            initializeResults.Success.Should().BeTrue();
            initializeResults.Results.Should().ContainSingle();

            cachedObject.Dispose();

            initializeResults = CachedObjectInitializer.InitializeAll();

            initializeResults.Success.Should().BeTrue();
            initializeResults.Results.Should().BeEmpty();
        }

        [Fact]
        public void InitializeDurationIsAccurate()
        {
            ICachedObject cachedObject;
            using (_setupLock.Enter())
            {
                cachedObject = CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return DateTime.UtcNow;
                    })
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Build();
            }

            var timer = Stopwatch.StartNew();
            
            var initializeResult = CachedObjectInitializer.InitializeAll();
            
            timer.Stop();
            
            cachedObject.Dispose();
            
            initializeResult.Success.Should().BeTrue();
            initializeResult.Results.Should().ContainSingle();
            initializeResult.Results[0].Duration
                .Should().BeGreaterThan(TimeSpan.FromSeconds(0.9))
                .And.BeLessThan(timer.Elapsed);
        }
        
        [Fact]
        public void NameIsSetOnInitializationResult()
        {
            var name = Guid.NewGuid().ToString();

            ICachedObject cachedObject;            
            using (_setupLock.Enter())
            {
                cachedObject = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMinutes(1))
                    .Named(name)
                    .Build();
            }

            var initializeResult = CachedObjectInitializer.InitializeAll();

            cachedObject.Dispose();
            
            initializeResult.Success.Should().BeTrue();
            initializeResult.Results.Should().ContainSingle();
            initializeResult.Results[0].Name.Should().Be(name);
        }
    }
}