using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Cron.Tests
{
    public class CronTests
    {
        [Theory]
        [InlineData("* * * * * *", 1)]
        [InlineData("*/2 * * * * *", 2)]
        public async Task CachedObjectRefreshSchedule(string cronExpression, int intervalSeconds)
        {
            var refreshResults = new List<CachedObjectRefreshResult>();

            var funcDuration = TimeSpan.FromMilliseconds(100);
            
            var date = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    await Task.Delay(funcDuration);
                    return DateTime.UtcNow;
                })
                .WithRefreshSchedule(cronExpression, true)
                .OnRefresh(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(10));

            var expectedCount = (10 / intervalSeconds) + 1;
            
            refreshResults.Count.Should().BeInRange(expectedCount - 1, expectedCount + 1);
            
            foreach (var result in refreshResults.Skip(1))
                result.Start.Millisecond.Should().NotBeInRange(250, 950);
        }
    }
}