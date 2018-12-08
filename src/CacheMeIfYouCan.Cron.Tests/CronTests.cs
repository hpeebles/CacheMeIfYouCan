using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshSchedule(cronExpression, true)
                .OnRefreshResult(refreshResults.Add)
                .Build(false);

            await date.Init();

            await Task.Delay(TimeSpan.FromSeconds(10));
            
            foreach (var result in refreshResults.Skip(2))
            {
                Assert.InRange(
                    result.Start - result.LastRefreshAttempt,
                    TimeSpan.FromSeconds(intervalSeconds - 0.2),
                    TimeSpan.FromSeconds(intervalSeconds + 0.2));
            }
        }
    }
}