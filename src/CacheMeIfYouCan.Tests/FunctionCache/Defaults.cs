using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class Defaults
    {
        [Fact]
        public async Task DefaultOnResultIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new List<FunctionCacheGetResult>();

            DefaultCacheSettings.OnResult = results.Add;
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = Guid.NewGuid().ToString();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }
            
            DefaultCacheSettings.OnResult = null;
        }
        
        [Fact]
        public async Task DefaultOnFetchIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new List<FunctionCacheFetchResult>();

            DefaultCacheSettings.OnFetch = results.Add;
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = Guid.NewGuid().ToString();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }

            DefaultCacheSettings.OnFetch = null;
        }
        
        [Fact]
        public async Task DefaultOnErrorIsTriggered()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var errors = new List<FunctionCacheErrorEvent>();

            DefaultCacheSettings.OnError = errors.Add;
            
            var cachedEcho = echo
                .Cached()
                .Build();

            var previousErrorCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var key = Guid.NewGuid().ToString();
                
                if (i % 2 == 0)
                {
                    await Assert.ThrowsAsync<Exception>(() => cachedEcho(key));
                    Assert.Equal(previousErrorCount += 2, errors.Count); // one for failing the fetch, one for failing the get
                    Assert.Equal(key, errors[errors.Count - 1].KeyString.Value);
                    Assert.Equal(key, errors[errors.Count - 2].KeyString.Value);
                }
                else
                {
                    Assert.Equal(key, await cachedEcho(key));
                }
            }
            
            DefaultCacheSettings.OnError = null;
        }
    }
}