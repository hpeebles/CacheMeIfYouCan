using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class Notifications
    {
        [Fact]
        public async Task OnRefreshResult()
        {
            var refreshResults = new List<CachedObjectRefreshResult<DateTime>>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            Assert.True(refreshResults.Count > 5);

            foreach (var result in refreshResults)
            {
                Assert.NotEqual(DateTime.MinValue, result.Start);
                Assert.True(result.Success);
                Assert.InRange(result.Duration, TimeSpan.FromTicks(1), TimeSpan.FromMilliseconds(10));
                Assert.NotEqual(DateTime.MinValue, result.NewValue);
            }
        }
    }
}