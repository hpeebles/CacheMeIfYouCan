using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class General
    {
        private readonly Random _rng = new Random();
        
        [Fact]
        public async Task ReturnsExpectedValue()
        {
            ITest impl = new TestImpl();

            var proxy = impl
                .Cached()
                .Build();

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
            ITest impl = new TestImpl();

            FunctionCacheGetResult lastResult = null;
            
            var proxy = impl
                .Cached()
                .OnResult(r => lastResult = r)
                .Build();

            for (var i = 0; i < 10; i++)
            {
                var randomString = Guid.NewGuid().ToString();

                for (var j = 0; j < 10; j++)
                {
                    Assert.Equal(randomString, await proxy.StringToString(randomString));
                    
                    Assert.Equal(j == 0 ? Outcome.Fetch : Outcome.FromCache, lastResult.Outcome);
                }
            }
        }
    }
}