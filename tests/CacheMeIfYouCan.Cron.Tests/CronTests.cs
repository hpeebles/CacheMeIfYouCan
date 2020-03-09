using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedObject;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Cron.Tests
{
    public class CronTests
    {
        [Theory]
        [InlineData("* * * * * *", 1)]
        [InlineData("*/2 * * * * *", 2)]
        public void WithRefreshSchedule_RefreshesValueAtExpectedTimes(string cronExpression, int intervalSeconds)
        {
            var countdown = new CountdownEvent(5);
            
            var refreshResults = new List<ValueRefreshedEvent<DateTime>>();

            var cachedObject = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    var start = DateTime.UtcNow;
                    await Task.Delay(50).ConfigureAwait(false);
                    return start;
                })
                .WithRefreshSchedule(cronExpression, true)
                .OnValueRefreshed(r =>
                {
                    if (r.IsResultOfInitialization)
                        return;
                    
                    refreshResults.Add(r);
                    if (!countdown.IsSet)
                        countdown.Signal();
                })
                .Build();

            cachedObject.Initialize();

            countdown.Wait(TimeSpan.FromSeconds(30));
            
            cachedObject.Dispose();
            
            refreshResults.Should().HaveCount(5);

            for (var i = 1; i < refreshResults.Count; i++)
            {
                var newValue = refreshResults[i].NewValue;
                newValue.Millisecond.Should().NotBeInRange(50, 980, "each refresh should happen at the start of a second");

                var previousValue = refreshResults[i - 1].NewValue;

                var interval = (newValue - previousValue);
                interval.Should().BeCloseTo(TimeSpan.FromSeconds(intervalSeconds), TimeSpan.FromMilliseconds(100));
            }
        }
    }
}