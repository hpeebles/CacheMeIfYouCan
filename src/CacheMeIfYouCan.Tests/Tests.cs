using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class Tests
    {
        [Fact]
        public async Task SubsequentCallsAreCached()
        {
            Func<string, Task<string>> echo = Echo;

            var fetchCount = 0;
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMinutes(1))
                .OnFetch(x => fetchCount++)
                .Build();

            var first = true;
            for (var i = 0; i < 10; i++)
            {
                var timer = Stopwatch.StartNew();
                var result = await cachedEcho("test!");
                
                Assert.Equal("test!", result);
                if (first)
                {
                    Assert.True(timer.Elapsed > TimeSpan.FromSeconds(1));
                    first = false;
                }
                else
                {
                    Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(10));
                }
            }
            
            Assert.Equal(1, fetchCount);
        }

        [Fact]
        public async Task AtMostOneActiveFetchPerKey()
        {
            Func<string, Task<string>> echo = Echo;

            var fetchCount = 0;

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMinutes(1))
                .OnFetch(x => fetchCount++)
                .Build();

            async Task<TimeSpan> MeasureDuration()
            {
                var timer = Stopwatch.StartNew();
                await cachedEcho("test!");
                return timer.Elapsed;
            }

            var tasks = Enumerable
                .Range(0, 100)
                .Select(id => MeasureDuration())
                .ToList();

            await Task.WhenAll(tasks);

            var timings = tasks
                .Select(t => t.Result)
                .ToList();
            
            Assert.Equal(1, fetchCount);
            Assert.True(timings.All(t => t > TimeSpan.FromSeconds(0.5)));
        }

        private async Task<string> Echo(string key)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            return key;
        }
    }
}