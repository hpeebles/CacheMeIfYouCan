using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class PendingRequestsCounter
    {
        [Fact]
        public async Task CountsAreCorrectForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Equal(0, GetPendingRequestsCount(name));

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            Assert.Equal(5, GetPendingRequestsCount(name));

            await Task.WhenAll(tasks);
            
            Assert.Equal(0, GetPendingRequestsCount(name));
        }
        
        [Fact]
        public async Task CountsAreCorrectForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(2))
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Equal(0, GetPendingRequestsCount(name));

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
                    Assert.Equal(expectedPendingRequestsCount, pendingRequestsCount);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    pendingRequestsCount = GetPendingRequestsCount(name);
                    Assert.Equal(expectedPendingRequestsCount, pendingRequestsCount);
                }
                
                if (pendingRequestsCount > maxPendingRequestsCount)
                    maxPendingRequestsCount = pendingRequestsCount;

                if (maxPendingRequestsCount > 0 && expectedPendingRequestsCount == 0)
                    break;
                
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            Task.WaitAll(tasks);
            
            Assert.Equal(0, GetPendingRequestsCount(name));
            Assert.True(maxPendingRequestsCount > 1);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => index++ % 2 == 0)
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Equal(0, GetPendingRequestsCount(name));

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            Assert.Equal(5, GetPendingRequestsCount(name));

            await Assert.ThrowsAnyAsync<Exception>(() => Task.WhenAll(tasks));

            Assert.Equal(0, GetPendingRequestsCount(name));
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(2), () => index++ % 2 == 0)
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Equal(0, GetPendingRequestsCount(name));

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
                    Assert.Equal(expectedPendingRequestsCount, pendingRequestsCount);
                }
                catch
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    pendingRequestsCount = GetPendingRequestsCount(name);
                    Assert.Equal(expectedPendingRequestsCount, pendingRequestsCount);
                }
                
                if (pendingRequestsCount > maxPendingRequestsCount)
                    maxPendingRequestsCount = pendingRequestsCount;

                if (maxPendingRequestsCount > 0 && expectedPendingRequestsCount == 0)
                    break;
                
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            await Assert.ThrowsAnyAsync<Exception>(() => Task.WhenAll(tasks));
            
            Assert.Equal(0, GetPendingRequestsCount(name));
            Assert.True(maxPendingRequestsCount > 1);
        }

        [Fact]
        public void CacheIsRemovedFromContainerOnceDisposedForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Single(PendingRequestsCounterContainer.GetCounts().Where(c => c.Name == name));
            
            cache.Dispose();
            
            Assert.Empty(PendingRequestsCounterContainer.GetCounts().Where(c => c.Name == name));
        }
        
        [Fact]
        public void CacheIsRemovedFromContainerOnceDisposedForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1))
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            Assert.Single(PendingRequestsCounterContainer.GetCounts().Where(c => c.Name == name));
            
            cache.Dispose();
            
            Assert.Empty(PendingRequestsCounterContainer.GetCounts().Where(c => c.Name == name));
        }

        private static int GetPendingRequestsCount(string name)
        {
            return PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name).Count;
        }
    }
}