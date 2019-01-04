using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class PendingRequestsCounter : CacheTestBase
    {
        [Fact]
        public async Task CountsAreCorrect()
        {
            var name = Guid.NewGuid().ToString();

            Func<string, Task<string>> echo = new Echo(k => TimeSpan.FromSeconds(1));
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached(name)
                    .Build();
            }

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => cachedEcho(i.ToString()))
                .ToArray();

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Task.WhenAll(tasks);
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
        }

        [Fact]
        public async Task CountsAreCorrectAfterExceptions()
        {
            var name = Guid.NewGuid().ToString();

            Func<string, Task<string>> echo = new Echo(k => TimeSpan.FromSeconds(1), k => Int32.Parse(k) % 2 == 0);
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached(name)
                    .Build();
            }

            var pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);

            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => cachedEcho(i.ToString()))
                .ToArray();

            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(10, pendingRequests.Count);

            await Assert.ThrowsAnyAsync<Exception>(() => Task.WhenAll(tasks));
            
            pendingRequests = PendingRequestsCounterContainer.GetCounts().Single(c => c.Name == name);
            
            Assert.Equal(0, pendingRequests.Count);
        }
    }
}