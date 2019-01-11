using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class MultiKey : CacheTestBase
    {
        [Fact]
        public async Task MultiKeyCachedIsProduced()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEcho(new[] { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ListParameterSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoList(new[] { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task SetParameterSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoSet(new HashSet<string> { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ConcurrentDictionaryReturnValueSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoToConcurrent(new[] { "123", "abc" });
            
            Assert.Single(results);
            Assert.Equal(2, results.Single().Results.Count);
        }
    }
}