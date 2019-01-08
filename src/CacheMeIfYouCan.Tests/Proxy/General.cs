using System;
using System.Collections.Generic;
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

        [Fact]
        public async Task WorksSyncAndAsync()
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

            var key = Guid.NewGuid().ToString();
            
            Assert.Equal(key, await proxy.StringToString(key));

            Assert.Single(results);
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            await proxy.StringToString(key);
            
            Assert.Equal(2, results.Count);
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
            
            Assert.Equal(key, proxy.StringToStringSync(key));

            Assert.Equal(3, results.Count);
            Assert.Equal(Outcome.Fetch, results.Last().Results.Single().Outcome);

            proxy.StringToStringSync(key);

            Assert.Equal(4, results.Count);
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);

        }
    }
}