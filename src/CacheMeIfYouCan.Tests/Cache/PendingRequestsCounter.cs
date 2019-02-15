using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class PendingRequestsCounter
    {
        private readonly CacheSetupLock _setupLock;

        public PendingRequestsCounter(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task CountsAreCorrectForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            Func<string, Task> func = str => cache.Get(new Key<string>(str, str));

            var task1 = func("1");

            await Task.Delay(100);
            
            GetPendingRequestsCount(name).Should().Be(1);

            var task2 = func("2");

            await Task.Delay(100);
            
            GetPendingRequestsCount(name).Should().Be(2);

            await Task.WhenAll(task1, task2);
            
            GetPendingRequestsCount(name).Should().Be(0);
        }
        
        [Fact]
        public async Task CountsAreCorrectForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1))
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            Func<string, Task> func = str => Task.Run(() => cache.Get(new Key<string>(str, str)));

            var task1 = func("1");

            await Task.Delay(100);
            
            GetPendingRequestsCount(name).Should().Be(1);

            var task2 = func("2");

            await Task.Delay(100);
            
            GetPendingRequestsCount(name).Should().Be(2);

            await Task.WhenAll(task1, task2);
            
            GetPendingRequestsCount(name).Should().Be(0);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => index++ % 2 == 1)
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            var key = new Key<string>("123", "123");

            Func<Task> func = () => cache.Get(key);

            for (var i = 0; i < 5; i++)
            {
                var task = i % 2 == 1
                    ? func.Should().ThrowAsync<Exception>()
                    : func();

                await Task.Delay(100);
                
                GetPendingRequestsCount(name).Should().Be(1);

                await task;

                GetPendingRequestsCount(name).Should().Be(0);
            }
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => index++ % 2 == 1)
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            var key = new Key<string>("123", "123");
            
            Func<Task> func = () => Task.Run(() => cache.Get(key));

            for (var i = 0; i < 5; i++)
            {
                var task = i % 2 == 1
                    ? func.Should().ThrowAsync<Exception>()
                    : func();

                await Task.Delay(100);
                
                GetPendingRequestsCount(name).Should().Be(1);

                await task;

                GetPendingRequestsCount(name).Should().Be(0);
            }
        }

        [Fact]
        public void CacheIsRemovedFromContainerOnceDisposedForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            PendingRequestsCounterContainer
                .GetCounts()
                .Where(c => c.Name == name)
                .Should()
                .ContainSingle();
            
            cache.Dispose();
            
            PendingRequestsCounterContainer
                .GetCounts()
                .Where(c => c.Name == name)
                .Should()
                .BeEmpty();
        }
        
        [Fact]
        public void CacheIsRemovedFromContainerOnceDisposedForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1))
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            PendingRequestsCounterContainer
                .GetCounts()
                .Where(c => c.Name == name)
                .Should()
                .ContainSingle();
            
            cache.Dispose();
            
            PendingRequestsCounterContainer
                .GetCounts()
                .Where(c => c.Name == name)
                .Should()
                .BeEmpty();
        }

        private static int GetPendingRequestsCount(string name)
        {
            return PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name).Count;
        }
    }
}