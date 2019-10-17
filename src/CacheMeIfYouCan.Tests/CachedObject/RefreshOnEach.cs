using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class RefreshOnEach
    {
        private readonly CachedObjectSetupLock _setupLock;

        public RefreshOnEach(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task ValueRefreshedWhenTriggered()
        {
            var updateResults = new List<CachedObjectSuccessfulUpdateResult>();
            var refreshTrigger = new Subject<bool>();

            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .RefreshOnEach(refreshTrigger)
                    .OnValueUpdated(updateResults.Add)
                    .Build();
            }

            await date.InitializeAsync();

            for (var i = 1; i < 10; i++)
            {
                updateResults.Should().HaveCount(i);
                refreshTrigger.OnNext(true);
                updateResults.Should().HaveCount(i + 1);
            }
        }
    }
}