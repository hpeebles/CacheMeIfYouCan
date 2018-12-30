using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    public class General
    {
        [Fact]
        public async Task RefreshedValueIsImmediatelyExposed()
        {
            var results = new List<CachedObjectRefreshResult>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .OnRefreshResult(r =>
                {
                    results.Add(r);
                    Assert.InRange(
                        r.NewValue,
                        DateTime.UtcNow.AddMilliseconds(-100),
                        DateTime.UtcNow.AddMilliseconds(100));
                })
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(10));
            
            date.Dispose();
            
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ContinuesToRefreshAfterException()
        {
            var index = 0;
            var refreshResults = new List<CachedObjectRefreshResult>();
            
            var date = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    if (index++ == 1)
                        throw new Exception();
                    
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                .OnRefreshResult(refreshResults.Add)
                .Build();

            await date.Initialize();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            Assert.True(refreshResults.Count > 2);

            for (var i = 0; i < refreshResults.Count; i++)
            {
                var result = refreshResults[i];
                
                if (i == 1)
                    Assert.False(result.Success);
                else
                    Assert.True(result.Success);
            }
        }

        [Fact]
        public async Task ThrowsIfAccessedAfterDisposed()
        {
            var date = CachedObjectFactory
                .ConfigureFor(() => DateTime.UtcNow)
                .WithRefreshInterval(TimeSpan.FromSeconds(1))
                .Build();

            await date.Initialize();

            Assert.True(date.Value > DateTime.MinValue);
            
            date.Dispose();

            Assert.Throws<ObjectDisposedException>(() => date.Value);
        }
    }
}