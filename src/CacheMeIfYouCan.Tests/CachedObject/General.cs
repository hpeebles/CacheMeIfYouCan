using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class General
    {
        private readonly CachedObjectSetupLock _setupLock;

        public General(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task RefreshedValueIsImmediatelyExposed()
        {
            var refreshResults = new List<CachedObjectSuccessfulUpdateResult>();

            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .OnValueUpdated(r =>
                    {
                        refreshResults.Add(r);
                        Assert.InRange(
                            r.NewValue,
                            DateTime.UtcNow.AddMilliseconds(-100),
                            DateTime.UtcNow.AddMilliseconds(100));
                    })
                    .Build();
            }

            await date.InitializeAsync();

            await Task.Delay(TimeSpan.FromSeconds(10));
            
            date.Dispose();
            
            Assert.NotEmpty(refreshResults);
        }

        [Fact]
        public async Task CachedObjectContinuesToRefreshAfterException()
        {
            var index = 0;
            var updateAttemptResults = new List<ICachedObjectUpdateAttemptResult>();

            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() =>
                    {
                        if (index++ == 1)
                            throw new Exception();

                        return DateTime.UtcNow;
                    })
                    .WithRefreshInterval(TimeSpan.FromMilliseconds(200))
                    .OnValueUpdated(updateAttemptResults.Add)
                    .OnException(updateAttemptResults.Add)
                    .Build();
            }

            await date.InitializeAsync();
            
            await Task.Delay(TimeSpan.FromSeconds(5));
            
            date.Dispose();
            
            updateAttemptResults.Count.Should().BeGreaterThan(2);

            for (var i = 0; i < updateAttemptResults.Count; i++)
            {
                var result = updateAttemptResults[i];
                
                if (i == 1)
                    result.Success.Should().BeFalse();
                else
                    result.Success.Should().BeTrue();
            }
        }

        [Fact]
        public async Task ThrowsIfAccessedAfterDisposed()
        {
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            await date.InitializeAsync();

            date.Value.Should().BeAfter(DateTime.MinValue);
            
            date.Dispose();

            Func<DateTime> act = () => date.Value;
            act.Should().Throw<ObjectDisposedException>();
        }

        [Fact]
        public async Task Named()
        {
            var refreshResults = new List<CachedObjectSuccessfulUpdateResult>();
            var name = Guid.NewGuid().ToString();
            
            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .WithRefreshInterval(TimeSpan.FromMilliseconds(1))
                    .Named(name)
                    .OnValueUpdated(refreshResults.Add)
                    .Build();
            }

            await date.InitializeAsync();

            await Task.Delay(TimeSpan.FromMilliseconds(500));

            date.Dispose();

            refreshResults.First().Name.Should().Be(name);
        }
    }
}