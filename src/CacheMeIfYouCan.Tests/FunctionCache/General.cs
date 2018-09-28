using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class General
    {
        [Fact]
        public async Task SubsequentCallsAreCached()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromSeconds(1));
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
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
        public async Task ShortTimeToLiveExpiresCorrectly()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMilliseconds(1))
                .OnResult(results.Add)
                .Build();
            
            for (var i = 0; i < 10; i++)
            {
                await cachedEcho("abc");

                await Task.Delay(5);
            }
            
            Assert.Equal(10, results.Count);
            Assert.True(results.All(r => r.Outcome == Outcome.Fetch));
        }
    }
}