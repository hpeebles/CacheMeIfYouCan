using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class Initialization
    {
        private readonly CachedObjectSetupLock _setupLock;

        public Initialization(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public void NotInitializedWillInitializeOnFirstCall()
        {
            ICachedObject<long> ticks;
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
            }

            var timer = Stopwatch.StartNew();

            ticks.Value.Should().BeGreaterThan(0);

            timer.Elapsed.Should().BeGreaterThan(TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public async Task CanBeInitializedDirectly()
        {
            ICachedObject<long> ticks;
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
            }

            await ticks.Initialize();

            var timer = Stopwatch.StartNew();

            ticks.Value.Should().BeGreaterThan(0);

            timer.Elapsed.Should().BeLessThan(TimeSpan.FromMilliseconds(20));
        }

        [Fact]
        public void InitializationOnlyRunsOnce()
        {
            var count = 0;
            
            ICachedObject<long> ticks;
            using (_setupLock.Enter())
            {
                ticks = CachedObjectFactory
                    .ConfigureFor(() =>
                    {
                        Interlocked.Increment(ref count);
                        return Task.Delay(100).ContinueWith(t => DateTime.UtcNow.Ticks);
                    })
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            var values = Enumerable
                .Range(0, 1000)
                .AsParallel()
                .Select(i => ticks.Value)
                .ToArray();

            count.Should().Be(1);
        }
    }
}