using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class RefreshInterval
    {
        private readonly CachedObjectSetupLock _setupLock;

        public RefreshInterval(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task ValueIsRefreshedAtRegularIntervals()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();

            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(4))
                    .OnRefresh(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(20));

            date.Dispose();
            
            var min = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Min();
            var max = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Max();

            min.Should().BeGreaterThan(TimeSpan.FromMilliseconds(3800));
            max.Should().BeLessThan(TimeSpan.FromMilliseconds(6000));
        }
        
        [Fact]
        public async Task JitterCausesRefreshIntervalsToFluctuate()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .WithJitterPercentage(50)
                    .OnRefresh(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(20));
            
            date.Dispose();
            
            var min = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Min();
            var max = refreshResults.Skip(1).Select(r => r.Start - r.LastRefreshAttempt).Max();
            
            min.Should().BeGreaterThan(TimeSpan.FromMilliseconds(500)).And.BeLessThan(TimeSpan.FromMilliseconds(900));
            max.Should().BeGreaterThan(TimeSpan.FromMilliseconds(1200));
        }

        [Fact]
        public async Task VariableRefreshInterval()
        {
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(r => TimeSpan.FromSeconds(r.SuccessfulRefreshCount))
                    .OnRefresh(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(14));
            
            date.Dispose();
            
            refreshResults.Count.Should().Be(5);
            foreach (var result in refreshResults.Skip(1))
            {
                var interval = result.Start - result.LastRefreshAttempt;
                    
                interval
                    .Should()
                    .BeGreaterThan(TimeSpan.FromSeconds(result.SuccessfulRefreshCount - 1.1))
                    .And
                    .BeLessThan(TimeSpan.FromSeconds(result.SuccessfulRefreshCount + 0.9));
            }
        }
    }
}