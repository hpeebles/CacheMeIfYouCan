using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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
                    .OnRefreshResult(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            Assert.True(refreshResults.Count > 2);

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