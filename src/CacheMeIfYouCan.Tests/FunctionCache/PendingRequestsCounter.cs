using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class PendingRequestsCounter
    {
        [Fact]
        public async Task CountsAreCorrect()
        {
            Func<string, Task<string>> echo = new Echo(k => TimeSpan.FromSeconds(1));

            var name = Guid.NewGuid().ToString();

            var cachedEcho = echo
                .Cached(name)
                .Build();

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
            Func<string, Task<string>> echo = new Echo(k => TimeSpan.FromSeconds(1), k => Int32.Parse(k) % 2 == 0);

            var name = Guid.NewGuid().ToString();

            var cachedEcho = echo
                .Cached(name)
                .Build();

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