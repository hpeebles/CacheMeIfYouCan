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

            var funcDuration = TimeSpan.FromMilliseconds(100);
            
            var date = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    await Task.Delay(funcDuration);
                    return DateTime.UtcNow;
                })
                .WithRefreshSchedule(cronExpression, true)
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(10));

            var expectedInterval = TimeSpan.FromSeconds(intervalSeconds) - funcDuration;
            
            foreach (var result in refreshResults.Skip(2))
            {
                Assert.InRange(
                    result.Start - result.LastRefreshAttempt,
                    expectedInterval - TimeSpan.FromMilliseconds(200),
                    expectedInterval + TimeSpan.FromMilliseconds(200));
            }
        }
    }
}