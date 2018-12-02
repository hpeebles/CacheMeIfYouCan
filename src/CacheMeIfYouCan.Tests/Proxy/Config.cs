using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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
                .WithTimeToLive(TimeSpan.FromMilliseconds(100))
                .OnResult(x => lastResult = x)
                .ConfigureFor<int, string>(x => x.IntToString, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)))
                .ConfigureFor<long, int>(x => x.LongToInt, c => c.WithTimeToLive(TimeSpan.FromSeconds(2)))
                .Build();

            // Run the functions with dummy data otherwise the first test usages will be slow
            await proxy.StringToString(String.Empty);
            await proxy.IntToString(0);
            await proxy.LongToInt(0);

            await proxy.StringToString("123");
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);

            await proxy.StringToString("123");
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            
            await proxy.StringToString("123");
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

            await Task.Delay(TimeSpan.FromSeconds(1));
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
        }
    }
}