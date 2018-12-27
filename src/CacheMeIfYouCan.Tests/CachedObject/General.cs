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
            
            var date = CachedObjectFactory
                .ConfigureFor(() =>
                {
                    if (index++ == 2)
                        throw new Exception();
                    
                    return DateTime.UtcNow;
                })
                .WithRefreshInterval(TimeSpan.FromMilliseconds(100))
                .Build();

            await date.Initialize();

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            Assert.True(DateTime.UtcNow - date.Value < TimeSpan.FromMilliseconds(200));
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