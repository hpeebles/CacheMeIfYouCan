using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class EnumerableKey
    {
        private readonly CacheSetupLock _setupLock;

        public EnumerableKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task EnumerableKeyCacheIsProduced()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEcho(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ListParameterSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoList(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task SetParameterSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoSet(new HashSet<string> { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ConcurrentDictionaryReturnValueSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            await proxy.MultiEchoToConcurrent(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }
    }
}