using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            var countdown = new CountdownEvent(3);
            
            var refreshDates = new List<DateTime>();
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    var start = DateTime.UtcNow;
                    await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
                    return start;
                })
                .WithRefreshSchedule(cronExpression, true)
                .OnValueRefresh(r =>
                {
                    if (r.IsResultOfInitialization)
                        return;
                    
                    refreshDates.Add(r.NewValue);
                    if (!countdown.IsSet)
                        countdown.Signal();
                })
                .Build();
            
            RunTest(cachedObject, countdown, refreshDates, intervalSeconds);
        }
        
        [Theory]
        [InlineData("* * * * * *", 1)]
        [InlineData("*/2 * * * * *", 2)]
        public void IncrementalCachedObject_WithRefreshSchedule_RefreshesValueAtExpectedTimes(string cronExpression, int intervalSeconds)
        {
            var countdown = new CountdownEvent(3);
            
            var refreshDates = new List<DateTime>();
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    var start = DateTime.UtcNow;
                    await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
                    return start;
                })
                .WithUpdates(_ => DateTime.UtcNow, (_, updates) => updates)
                .WithRefreshSchedule(cronExpression, true)
                .OnValueRefresh(r =>
                {
                    if (r.IsResultOfInitialization)
                        return;
                    
                    refreshDates.Add(r.NewValue);
                    if (!countdown.IsSet)
                        countdown.Signal();
                })
                .Build();
            
            RunTest(cachedObject, countdown, refreshDates, intervalSeconds);
        }
        
        [Theory]
        [InlineData("* * * * * *", 1)]
        [InlineData("*/2 * * * * *", 2)]
        public void UpdateableCachedObject_WithRefreshSchedule_RefreshesValueAtExpectedTimes(string cronExpression, int intervalSeconds)
        {
            var countdown = new CountdownEvent(3);
            
            var refreshDates = new List<DateTime>();
            
            var cachedObject = CachedObjectFactory
                .ConfigureFor(async () =>
                {
                    var start = DateTime.UtcNow;
                    await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
                    return start;
                })
                .WithUpdates<bool>((_, __) => DateTime.UtcNow)
                .WithRefreshSchedule(cronExpression, true)
                .OnValueRefresh(r =>
                {
                    if (r.IsResultOfInitialization)
                        return;
                    
                    refreshDates.Add(r.NewValue);
                    if (!countdown.IsSet)
                        countdown.Signal();
                })
                .Build();
            
            RunTest(cachedObject, countdown, refreshDates, intervalSeconds);
        }
        
        [Theory]
        [InlineData("* * * * * *", 1)]
        [InlineData("*/2 * * * * *", 2)]
        public void WithUpdateSchedule_UpdatesValueAtExpectedTimes(string cronExpression, int intervalSeconds)
        {
            var countdown = new CountdownEvent(3);
            
            var updateDates = new List<DateTime>();

            var cachedObject = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithUpdatesAsync(async _ =>
                {
                    var start = DateTime.UtcNow;
                    await Task.Delay(TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
                    return start;
                }, (current, next) => Task.FromResult(next))
                .WithUpdateSchedule(cronExpression, true)
                .OnValueUpdate(e =>
                {
                    updateDates.Add(e.NewValue);
                    if (!countdown.IsSet)
                        countdown.Signal();
                })
                .Build();

            RunTest(cachedObject, countdown, updateDates, intervalSeconds);
        }

        private static void RunTest(
            ICachedObject<DateTime> cachedObject,
            CountdownEvent countdown,
            List<DateTime> refreshOrUpdateDates,
            int intervalSeconds)
        {
            cachedObject.Initialize();

            countdown.Wait(TimeSpan.FromSeconds(15)).Should().BeTrue();
            
            cachedObject.Dispose();
            
            refreshOrUpdateDates.Should().HaveCount(3);

            for (var i = 1; i < refreshOrUpdateDates.Count; i++)
            {
                var newValue = refreshOrUpdateDates[i];
                newValue.Millisecond.Should().NotBeInRange(50, 980, "each refresh should happen at the start of a second");

                var previousValue = refreshOrUpdateDates[i - 1];

                var interval = newValue - previousValue;
                interval.Should().BeCloseTo(TimeSpan.FromSeconds(intervalSeconds), TimeSpan.FromMilliseconds(100));
            }
        }
    }
}