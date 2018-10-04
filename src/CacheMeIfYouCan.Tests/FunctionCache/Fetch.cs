using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class Fetch
    {
        [Fact]
        public async Task AtMostOneActiveFetchPerKey()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));

            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();

            var cachedEcho = echo
                .Cached()
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
            
            Assert.Equal(1, fetches.Count(f => !f.Results.Single().Duplicate));
            Assert.True(timings.All(t => t > TimeSpan.FromSeconds(0.5)));
        }
        
        [Fact]
        public async Task EarlyFetchEnabledCausesValuesToBeFetchedEarly()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromMilliseconds(100));

            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();

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
                .Where(f => f.Reason == FetchReason.EarlyFetch)
                .ToList();
            
            Assert.NotEmpty(earlyFetches);
        }
        
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task FetchDurationIsAccurate(int seconds)
        {
            var duration = TimeSpan.FromSeconds(seconds);
            
            Func<string, Task<string>> echo = new Echo(duration);
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
                .DisableCache()
                .OnFetch(fetches.Add)
                .Build();
            
            await cachedEcho("abc");
            
            Assert.Single(fetches);
            Assert.True(duration.Ticks < fetches[0].Duration && fetches[0].Duration < duration.Ticks * 1.1);
        }
        
        [Fact]
        public async Task FetchTimestampIsAccurate()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnFetch(fetches.Add)
                .Build();
            
            var now = Timestamp.Now;
            
            await cachedEcho("abc");
            
            Assert.Single(fetches);
            Assert.True(now <= fetches[0].Start && fetches[0].Start < now + TimeSpan.FromMilliseconds(10).Ticks);
        }
    }
}