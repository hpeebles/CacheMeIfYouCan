using System;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class Config
    {
        [Fact]
        public async Task ConfigureForTests()
        {
            ITest impl = new TestImpl();

            FunctionCacheGetResult lastResult = null;
            
            var proxy = impl
                .Cached()
                .For(TimeSpan.FromMilliseconds(100))
                .OnResult(x => lastResult = x)
                .ConfigureFor<int, string>(x => x.IntToString, c => c.For(TimeSpan.FromSeconds(1)))
                .ConfigureFor<double, double>(x => x.DoubleToDouble, c => c.For(TimeSpan.FromSeconds(2)))
                .Build();

            await proxy.StringToString("123");
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);
            
            await proxy.DoubleToDouble(123);
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);

            await proxy.StringToString("123");
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);
            
            await proxy.DoubleToDouble(123);
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            
            await proxy.StringToString("123");
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);
            
            await proxy.DoubleToDouble(123);
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);
            
            await proxy.DoubleToDouble(123);
            Assert.Equal(Outcome.FromCache, lastResult.Outcome);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            await proxy.DoubleToDouble(123);
            Assert.Equal(Outcome.Fetch, lastResult.Outcome);
        }
    }
}