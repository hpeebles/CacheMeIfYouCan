using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class MultiKey
    {
        [Fact]
        public async Task MultiKeyCachedIsProduced()
        {
            ITest impl = new TestImpl();

            var results = new List<FunctionCacheGetResult>();
            
            var proxy = impl
                .Cached()
                .OnResult(results.Add)
                .Build();
            
            await proxy.MultiEcho(new[] { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count());
        }

        [Fact]
        public async Task ListParameterSucceeds()
        {
            ITest impl = new TestImpl();
            
            var results = new List<FunctionCacheGetResult>();
            
            var proxy = impl
                .Cached()
                .OnResult(results.Add)
                .Build();
            
            await proxy.MultiEchoList(new[] { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count());
        }
    }
}