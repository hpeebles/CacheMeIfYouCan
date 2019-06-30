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
            var refreshResults = new List<CachedObjectUpdateResult>();
            var refreshTrigger = new Subject<bool>();

            ICachedObject<DateTime> date;
            using (_setupLock.Enter())
            {
                date = CachedObjectFactory
                    .ConfigureFor(() => DateTime.UtcNow)
                    .RefreshOnEach(refreshTrigger)
                    .OnUpdate(refreshResults.Add)
                    .Build();
            }

            await date.Initialize();

            for (var i = 1; i < 10; i++)
            {
                refreshResults.Should().HaveCount(i);
                refreshTrigger.OnNext(true);
                refreshResults.Should().HaveCount(i + 1);
            }
        }
    }
}