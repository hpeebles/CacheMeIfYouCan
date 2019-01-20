using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class Notifications
    {
        private readonly CachedObjectSetupLock _setupLock;

        public Notifications(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task OnRefreshResult()
        {
            var refreshResults = new List<CachedObjectRefreshResult<DateTime>>();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                    .OnRefresh(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            refreshResults.Count.Should().BeGreaterThan(2);

            foreach (var result in refreshResults)
            {
                result.Start.Should().BeAfter(DateTime.MinValue);
                result.Success.Should().BeTrue();
                result.NewValue.Should().BeAfter(DateTime.MinValue);
                result.Duration
                    .Should().BeGreaterThan(TimeSpan.FromTicks(1))
                    .And.BeLessThan(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}