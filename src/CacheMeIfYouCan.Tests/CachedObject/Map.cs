using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.CachedObject
{
    [Collection(TestCollections.CachedObject)]
    public class Map
    {
        private readonly CachedObjectSetupLock _setupLock;

        public Map(CachedObjectSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task MappedCachedObjectIsUpdatedWhenSourceIsUpdated()
        {
            var refreshTrigger = new Subject<bool>();

            var count = 0;
            
            ICachedObject<int> counter;
            using (_setupLock.Enter())
            {
                counter = CachedObjectFactory
                    .ConfigureFor(() => count++)
                    .RefreshOnEach(refreshTrigger)
                    .Build();
            }

            var multipliedByTwo = counter.Map(c => c * 2);

            await counter.InitializeAsync();
            
            for (var i = 1; i < 10; i++)
            {
                refreshTrigger.OnNext(true);
                counter.Value.Should().Be(i);
                multipliedByTwo.Value.Should().Be(i * 2);
            }
            
            counter.Dispose();
        }
        
        [Fact]
        public async Task InitializingMappedCachedObjectWillInitializeSource()
        {
            var refreshTrigger = new Subject<bool>();

            var count = 0;
            
            ICachedObject<int> counter;
            using (_setupLock.Enter())
            {
                counter = CachedObjectFactory
                    .ConfigureFor(() => count++)
                    .RefreshOnEach(refreshTrigger)
                    .Build();
            }

            var multipliedByTwo = counter.Map(c => c * 2);

            multipliedByTwo.State.Should().Be(CachedObjectState.PendingInitialization);
            counter.State.Should().Be(CachedObjectState.PendingInitialization);

            await multipliedByTwo.InitializeAsync();

            multipliedByTwo.State.Should().Be(CachedObjectState.Ready);
            counter.State.Should().Be(CachedObjectState.Ready);
            
            counter.Dispose();
        }
        
        [Fact]
        public async Task SingleSourceCanCreateMultipleMappedCachedObjects()
        {
            var refreshTrigger = new Subject<bool>();

            var count = 0;
            
            ICachedObject<int> counter;
            using (_setupLock.Enter())
            {
                counter = CachedObjectFactory
                    .ConfigureFor(() => count++)
                    .RefreshOnEach(refreshTrigger)
                    .Build();
            }

            var multipliedByTwo = counter.Map(c => c * 2);
            var multipliedByThree = counter.Map(c => c * 3);
            var multipliedByFour = counter.Map(c => c * 4);

            await counter.InitializeAsync();

            for (var i = 1; i < 10; i++)
            {
                refreshTrigger.OnNext(true);
                counter.Value.Should().Be(i);
                multipliedByTwo.Value.Should().Be(i * 2);
                multipliedByThree.Value.Should().Be(i * 3);
                multipliedByFour.Value.Should().Be(i * 4);
            }

            counter.Dispose();
        }
    }
}