using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class RefreshInterval
    {
        [Fact]
        public async Task ValueIsRefreshedAtRegularIntervals()
        {
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .RefreshInterval(TimeSpan.FromMilliseconds(200))
                .Build(false);

            await date.Init();

            var results = new HashSet<DateTime>();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                results.Add(date.Value);

                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            var sorted = results
                .OrderBy(r => r)
                .ToArray();
            
            var diffs = sorted
                .Zip(sorted.Skip(1), (l, r) => r - l)
                .ToArray();
            
            var min = diffs.Min();
            var max = diffs.Max();
            
            Assert.True(TimeSpan.FromMilliseconds(180) <= min);
            Assert.True(max <= TimeSpan.FromMilliseconds(250));
        }
        
        [Fact]
        public async Task JitterCausesRefreshIntervalsToFluctuate()
        {
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .RefreshInterval(TimeSpan.FromMilliseconds(200))
                .JitterPercentage(50)
                .Build(false);

            await date.Init();

            var results = new HashSet<DateTime>();

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            while (!cancellationTokenSource.IsCancellationRequested)
            {
                results.Add(date.Value);

                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            var sorted = results
                .Skip(1) // skip first one as it is always slow due to JITting
                .OrderBy(r => r)
                .ToArray();
            
            var diffs = sorted
                .Zip(sorted.Skip(1), (l, r) => r - l)
                .ToArray();

            var min = diffs.Min();
            var max = diffs.Max();
            
            Assert.True(TimeSpan.FromMilliseconds(100) <= min && min <= TimeSpan.FromMilliseconds(150));
            Assert.True(TimeSpan.FromMilliseconds(250) <= max && max <= TimeSpan.FromMilliseconds(350));
        }
    }
}