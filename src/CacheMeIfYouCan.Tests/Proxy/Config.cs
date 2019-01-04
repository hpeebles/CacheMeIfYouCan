using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class Config : CacheTestBase
    {
        [Fact]
        public async Task ConfigureForTests()
        {
            FunctionCacheGetResult lastResult = null;

            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMilliseconds(500))
                    .OnResult(x => lastResult = x)
                    .ConfigureFor<int, string>(x => x.IntToString, c => c.WithTimeToLive(TimeSpan.FromSeconds(1)))
                    .ConfigureFor<long, int>(x => x.LongToInt, c => c.WithTimeToLive(TimeSpan.FromSeconds(2)))
                    .Build();
            }

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

            var timer = Stopwatch.StartNew();

            while (timer.Elapsed < TimeSpan.FromMilliseconds(400))
            {
                await proxy.StringToString("123");
                Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            var untilStringToStringExpires = TimeSpan.FromMilliseconds(500) - timer.Elapsed;

            if (untilStringToStringExpires.Ticks > 0)
                await Task.Delay(untilStringToStringExpires);
            
            await proxy.StringToString("123");
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);

            while (timer.Elapsed < TimeSpan.FromMilliseconds(900))
            {
                await proxy.IntToString(123);
                Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            var untilIntToStringExpires = TimeSpan.FromSeconds(1) - timer.Elapsed;

            if (untilIntToStringExpires.Ticks > 0)
                await Task.Delay(untilIntToStringExpires);
            
            await proxy.IntToString(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
            
            while (timer.Elapsed < TimeSpan.FromMilliseconds(1900))
            {
                await proxy.LongToInt(123);
                Assert.Equal(Outcome.FromCache, lastResult.Results.Single().Outcome);

                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            var untilLongToIntExpires = TimeSpan.FromSeconds(2) - timer.Elapsed;

            if (untilLongToIntExpires.Ticks > 0)
                await Task.Delay(untilIntToStringExpires);
            
            await proxy.LongToInt(123);
            Assert.Equal(Outcome.Fetch, lastResult.Results.Single().Outcome);
        }
    }
}