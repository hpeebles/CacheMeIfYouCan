using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class MultiParam
    {
        private readonly CacheSetupLock _setupLock;

        public MultiParam(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task MultiParamCacheSucceeds()
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

            var result = await proxy.MultiParamEcho("123", 123);
            
            Assert.Equal("123_123", result);
            Assert.Single(results);
            Assert.Single(results.Single().Results);
        }
    }
}