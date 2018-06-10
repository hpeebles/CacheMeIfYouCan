using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            Func<string, Task<string>> echo = x => Echo(x, TimeSpan.FromSeconds(1));

            var fetches = new ConcurrentBag<FunctionCacheFetchResult<string>>();
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMinutes(1))
                .OnFetch(fetches.Add)
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
            
            Assert.Single(fetches);
        }

        [Fact]
        public async Task AtMostOneActiveFetchPerKey()
        {
            Func<string, Task<string>> echo = x => Echo(x, TimeSpan.FromSeconds(1));

            var fetches = new ConcurrentBag<FunctionCacheFetchResult<string>>();

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMinutes(1))
                .OnFetch(fetches.Add)
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
            
            Assert.Equal(1, fetches.Count(f => !f.Duplicate));
            Assert.True(timings.All(t => t > TimeSpan.FromSeconds(0.5)));
        }

        [Fact]
        public async Task WithEarlyFetchEnabledValuesAreFetchedEarly()
        {
            Func<string, Task<string>> echo = x => Echo(x, TimeSpan.FromMilliseconds(100));

            var fetches = new ConcurrentBag<FunctionCacheFetchResult<string>>();

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromSeconds(1))
                .OnFetch(fetches.Add)
                .Build();

            async Task<TimeSpan> MeasureDuration()
            {
                var timer = Stopwatch.StartNew();
                await cachedEcho("test!");
                return timer.Elapsed;
            }

            var tasks = new List<Task>();
            foreach (var id in Enumerable.Range(0, 250))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                tasks.Add(MeasureDuration());
            }

            await Task.WhenAll(tasks);

            var earlyFetches = fetches
                .Where(f => f.ExistingTtl.HasValue)
                .ToList();
            
            Assert.NotEmpty(earlyFetches);
        }

        private async Task<string> Echo(string key, TimeSpan delay)
        {
            await Task.Delay(delay);

            return key;
        }
    }
}