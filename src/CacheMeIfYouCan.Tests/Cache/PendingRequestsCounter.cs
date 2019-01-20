using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            GetPendingRequestsCount(name).Should().Be(5);

            await Task.WhenAll(tasks);
            
            GetPendingRequestsCount(name).Should().Be(0);
        }
        
        [Fact]
        public async Task CountsAreCorrectForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(2))
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            var expectedPendingRequestsCount = 0;
            
            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => Task.Run(() =>
                {
                    Interlocked.Increment(ref expectedPendingRequestsCount);
                    cache.Get(new Key<string>(k, k));
                    Interlocked.Decrement(ref expectedPendingRequestsCount);
                }))
                .ToArray();

            var timer = Stopwatch.StartNew();
            
            var maxPendingRequestsCount = 0;

            while (timer.Elapsed < TimeSpan.FromSeconds(10))
            {
                var pendingRequestsCount = GetPendingRequestsCount(name);

                try
                {
                    // This can fail if the check is done during the tiny gap between the expectedPendingRequestsCount
                    // being incremented and the actual pending requests count being incremented
                    pendingRequestsCount.Should().Be(expectedPendingRequestsCount);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    pendingRequestsCount = GetPendingRequestsCount(name);
                    pendingRequestsCount.Should().Be(expectedPendingRequestsCount);
                }
                
                if (pendingRequestsCount > maxPendingRequestsCount)
                    maxPendingRequestsCount = pendingRequestsCount;

                if (maxPendingRequestsCount > 0 && expectedPendingRequestsCount == 0)
                    break;
                
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            Task.WaitAll(tasks);
            
            GetPendingRequestsCount(name).Should().Be(0);
            maxPendingRequestsCount.Should().BeGreaterThan(1);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => index++ % 2 == 0)
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            GetPendingRequestsCount(name).Should().Be(5);

            Func<Task> func = () => Task.WhenAll(tasks);
            await func.Should().ThrowAsync<Exception>();

            GetPendingRequestsCount(name).Should().Be(0);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(2), () => index++ % 2 == 0)
                    .WithPendingRequestsCounter()
                    .Build<string, string>(name);
            }

            GetPendingRequestsCount(name).Should().Be(0);

            var expectedPendingRequestsCount = 0;
            
            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => Task.Run(() =>
                {
                    Interlocked.Increment(ref expectedPendingRequestsCount);
                    try
                    {
                        cache.Get(new Key<string>(k, k));
                    }
                    finally
                    {
                        Interlocked.Decrement(ref expectedPendingRequestsCount);
                    }
                }))
                .ToArray();

            var timer = Stopwatch.StartNew();
            
            var maxPendingRequestsCount = 0;

            while (timer.Elapsed < TimeSpan.FromSeconds(10))
            {
                var pendingRequestsCount = GetPendingRequestsCount(name);
                
                try
                {
                    // This can fail if the check is done during the tiny gap between the expectedPendingRequestsCount
                    // being incremented and the actual pending requests count being incremented
                    expectedPendingRequestsCount.Should().Be(pendingRequestsCount);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    pendingRequestsCount = GetPendingRequestsCount(name);
                    expectedPendingRequestsCount.Should().Be(pendingRequestsCount);
                }
                
                if (pendingRequestsCount > maxPendingRequestsCount)
                    maxPendingRequestsCount = pendingRequestsCount;

                if (maxPendingRequestsCount > 0 && expectedPendingRequestsCount == 0)
                    break;
                
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            Func<Task> func = () => Task.WhenAll(tasks);
            await func.Should().ThrowAsync<Exception>();
            
            GetPendingRequestsCount(name).Should().Be(0);
            maxPendingRequestsCount.Should().BeGreaterThan(1);
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