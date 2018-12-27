using System;
using System.Linq;
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

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Task.WhenAll(tasks);
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
        }
        
        [Fact]
        public async Task CountsAreCorrectForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(3))
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .Select(k => Task.Run(() => cache.Get(new Key<string>(k, k))))
                .ToArray();

            await Task.Delay(TimeSpan.FromSeconds(2));

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Task.WhenAll(tasks);
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForDistributedCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => index++ % 2 == 0)
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .Select(k => cache.Get(new Key<string>(k, k)))
                .ToArray();

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Assert.ThrowsAnyAsync<Exception>(() => Task.WhenAll(tasks));
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
        }
        
        [Fact]
        public async Task CountsAreCorrectAfterExceptionsForLocalCache()
        {
            var name = Guid.NewGuid().ToString();
            var index = 0;
            
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(3), () => index++ % 2 == 0)
                .WithPendingRequestsCounter()
                .Build<string, string>(name);

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .Select(k => Task.Run(() => cache.Get(new Key<string>(k, k))))
                .ToArray();

            await Task.Delay(TimeSpan.FromSeconds(2));

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Assert.ThrowsAnyAsync<Exception>(() => Task.WhenAll(tasks));
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
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
    }
}