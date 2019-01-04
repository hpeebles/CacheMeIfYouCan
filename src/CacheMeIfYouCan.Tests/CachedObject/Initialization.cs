using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class Initialization : CachedObjectTestBase
    {
        [Fact]
        public void NotInitializedWillInitializeOnFirstCall()
        {
            ICachedObject<long> ticks;
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
            }

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed >= TimeSpan.FromSeconds(1));
        }
        
        [Fact]
        public async Task CanBeInitializedDirectly()
        {
            ICachedObject<long> ticks;
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
            }

            await ticks.Initialize();

            var timer = Stopwatch.StartNew();

            Assert.NotEqual(0, ticks.Value);

            Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(20));
        }

        [Fact]
        public void InitializationOnlyRunsOnce()
        {
            var count = 0;
            
            ICachedObject<long> ticks;
            using (EnterSetup(false))
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

            Assert.Equal(1, count);
        }
    }
}