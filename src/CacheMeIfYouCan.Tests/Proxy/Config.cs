using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class Config
    {
        private readonly CacheSetupLock _setupLock;

        public Config(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task ConfigureForTests()
        {
            var results1 = new List<FunctionCacheGetResult>();
            var results2 = new List<FunctionCacheGetResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .ConfigureFor<int, string>(x => x.IntToString, c => c.OnResult(results1.Add))
                    .ConfigureFor<long, int>(x => x.LongToInt, c => c.OnResult(results2.Add))
                    .Build();
            }

            await proxy.StringToString("123");
            
            Assert.Empty(results1);
            Assert.Empty(results2);
            
            await proxy.IntToString(0);

            Assert.Single(results1);
            Assert.Empty(results2);
            
            await proxy.LongToInt(0);

            Assert.Single(results1);
            Assert.Single(results2);
        }
    }
}