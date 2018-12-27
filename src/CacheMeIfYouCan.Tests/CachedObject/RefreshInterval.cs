using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class RefreshInterval
    {
        [Fact]
        public async Task ValueIsRefreshedAtRegularIntervals()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(2))
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(20));

            date.Dispose();
            
            var min = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Min();
            var max = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Max();

            Assert.True(TimeSpan.FromMilliseconds(1800) <= min);
            Assert.True(max <= TimeSpan.FromMilliseconds(3000));
        }
        
        [Fact]
        public async Task JitterCausesRefreshIntervalsToFluctuate()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .WithJitterPercentage(50)
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(20));
            
            date.Dispose();
            
            var min = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Min();
            var max = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Max();
            
            Assert.InRange(min, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(800));
            Assert.InRange(max, TimeSpan.FromMilliseconds(1200), TimeSpan.FromMilliseconds(2000));
        }

        [Fact]
        public async Task VariableRefreshInterval()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(r => TimeSpan.FromSeconds(r.SuccessfulRefreshCount))
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(12));
            
            date.Dispose();
            
            Assert.Equal(5, refreshResults.Count);
            foreach (var result in refreshResults.Skip(1))
            {
                Assert.InRange(
                    result.Start - result.LastRefreshAttempt,
                    TimeSpan.FromSeconds(result.SuccessfulRefreshCount - 1.1),
                    TimeSpan.FromSeconds(result.SuccessfulRefreshCount));
            }
        }
    }
}