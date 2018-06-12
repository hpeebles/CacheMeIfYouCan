using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CacheMeIfYouCan.Tests
{
    public class Errors
    {
        [Fact]
        public async Task OnErrorIsTriggered()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var errors = new List<FunctionCacheErrorEvent<string>>();
            
            var cachedEcho = echo
                .Cached()
                .OnError(errors.Add)
                .Build();

            var previousErrorCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var key = Guid.NewGuid().ToString();
                
                if (i % 2 == 0)
                {
                    await Assert.ThrowsAsync<Exception>(() => cachedEcho(key));
                    Assert.Equal(previousErrorCount += 2, errors.Count); // one for failing the fetch, one for failing the get
                    Assert.Equal(key, errors[errors.Count - 1].Key);
                    Assert.Equal(key, errors[errors.Count - 2].Key);
                }
                else
                {
                    Assert.Equal(key, await cachedEcho(key));
                }
            }
        }
        
        [Fact]
        public async Task ReturnsNullIfContinueOnExceptionIsSet()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMilliseconds(1))
                .ContinueOnException()
                .Build();

            for (var i = 0; i < 10; i++)
            {
                var result = await cachedEcho("abc");
                
                if (i % 2 == 1)
                    Assert.Equal("abc", result);
                else
                    Assert.Null(result);

                await Task.Delay(5);
            }
        }
        
        [Fact]
        public async Task ReturnsDefaultValueIfContinueOnExceptionIsSet()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMilliseconds(1))
                .ContinueOnException("defaultValue")
                .Build();

            for (var i = 0; i < 10; i++)
            {
                var result = await cachedEcho("abc");
                
                if (i % 2 == 1)
                    Assert.Equal("abc", result);
                else
                    Assert.Equal("defaultValue", result);

                await Task.Delay(5);
            }
        }
        
        [Fact]
        public async Task CacheStillWorksForOtherKeys()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => x.Equals("error!"));

            var errors = new List<FunctionCacheErrorEvent<string>>();
            var fetches = new List<FunctionCacheFetchResult<string, string>>();

            var cachedEcho = echo
                .Cached()
                .OnError(errors.Add)
                .OnFetch(fetches.Add)
                .Build();

            var keys = new[] { "one", "error!", "two" };

            var results = new List<KeyValuePair<string, string>>();

            var loopCount = 5;
            var thrownErrorsCount = 0;

            for (var i = 0; i < loopCount; i++)
            {
                foreach (var key in keys)
                {
                    try
                    {
                        var value = await cachedEcho(key);

                        results.Add(new KeyValuePair<string, string>(key, value));
                    }
                    catch
                    {
                        thrownErrorsCount++;
                    }
                }
            }

            Assert.Equal(loopCount * 2, errors.Count);
            Assert.True(errors.All(k => k.Key == "error!"));
            Assert.Equal(loopCount, thrownErrorsCount);
            Assert.Equal(loopCount, results.Count(kv => kv.Key == "one"));
            Assert.Equal(loopCount, results.Count(kv => kv.Key == "two"));
            Assert.Equal(0, results.Count(kv => kv.Key == "error!"));
            Assert.Equal(2, fetches.Count(f => f.Success));
            Assert.Equal(0, fetches.Count(f => f.Duplicate));
        }

        [Fact]
        public async Task CacheStillWorksForSubsequentCallsToSameKey()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.FromMilliseconds(10), x => count++ % 4 == 2);

            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMilliseconds(1))
                .Build();

            for (var i = 0; i < 20; i++)
            {
                if (i % 4 == 2)
                    await Assert.ThrowsAsync<Exception>(() => cachedEcho("abc"));
                else
                    Assert.Equal("abc", await cachedEcho("abc"));

                await Task.Delay(5);
            }
        }
    }
}