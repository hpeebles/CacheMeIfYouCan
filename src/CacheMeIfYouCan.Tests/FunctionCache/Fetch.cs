using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class Fetch
    {
        private readonly CacheSetupLock _setupLock;

        public Fetch(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task AtMostOneActiveFetchPerKeyWhenDuplicateRequestCatchingIsEnabled()
        {
            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .CatchDuplicateRequests()
                    .OnFetch(fetches.Add)
                    .Build();
            }

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
        
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task FetchDurationIsAccurate(int seconds)
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            var duration = TimeSpan.FromSeconds(seconds);
            
            Func<string, Task<string>> echo = new Echo(duration);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .DisableCache()
                    .OnFetch(fetches.Add)
                    .Build();
            }

            await cachedEcho("warmup");
            
            fetches.Clear();
            
            await cachedEcho("abc");
            
            fetches.Should().ContainSingle();
            Assert.InRange(fetches[0].Duration.Ticks, duration.Ticks * 0.99, duration.Ticks * 1.1);
        }
        
        [Fact]
        public async Task FetchTimestampIsAccurate()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnFetch(fetches.Add)
                    .Build();
            }

            var start = DateTime.UtcNow;
            
            await cachedEcho("abc");
            
            fetches.Should().ContainSingle();
            fetches[0].Start.Should().BeCloseTo(start, TimeSpan.FromMilliseconds(100));
        }
    }
}