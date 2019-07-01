using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class State
    {
        private readonly CachedObjectSetupLock _setupLock;

        public State(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task StateIsUpdatedCorrectly()
        {
            ICachedObject<DateTime> cachedObject;
            using (_setupLock.Enter())
            {
                cachedObject = CachedObjectFactory
                    .ConfigureFor(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        return DateTime.UtcNow;
                    })
                    .WithRefreshInterval(TimeSpan.FromSeconds(1))
                    .Build();
            }

            cachedObject.State.Should().Be(CachedObjectState.PendingInitialization);

            var task = cachedObject.Initialize();

            cachedObject.State.Should().Be(CachedObjectState.InitializationInProgress);

            await task;

            cachedObject.State.Should().Be(CachedObjectState.Ready);

            cachedObject.Dispose();

            cachedObject.State.Should().Be(CachedObjectState.Disposed);
        }
    }
}