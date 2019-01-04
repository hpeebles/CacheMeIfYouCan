using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class General : CacheTestBase
    {
        private readonly Random _rng = new Random();
        
        [Fact]
        public async Task ReturnsExpectedValue()
        {
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .Build();
            }

            var randomString = Guid.NewGuid().ToString();
            Assert.Equal(randomString, await proxy.StringToString(randomString));

            var randomInt = _rng.Next(1000);
            Assert.Equal(randomInt.ToString(), await proxy.IntToString(randomInt));

            var randomLong = _rng.Next(1000);
            Assert.Equal(randomLong * 2, await proxy.LongToInt(randomLong));
        }

        [Fact]
        public async Task IsCached()
        {
            FunctionCacheGetResult lastResult = null;
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResult(r => lastResult = r)
                    .Build();
            }

            for (var i = 0; i < 10; i++)
            {
                var randomString = Guid.NewGuid().ToString();

                for (var j = 0; j < 10; j++)
                {
                    Assert.Equal(randomString, await proxy.StringToString(randomString));
                    
                    Assert.Equal(j == 0 ? Outcome.Fetch : Outcome.FromCache, lastResult.Results.Single().Outcome);
                }
            }
        }
    }
}