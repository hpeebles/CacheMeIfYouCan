using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class Defaults
    {
        private const string KeyPrefix = "DefaultsTests";
        
        [Fact]
        public async Task DefaultOnResultIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new List<FunctionCacheGetResult>();

            DefaultCacheConfig.Configuration.OnResult = x =>
            {
                if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                    results.Add(x);
            };
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }
            
            DefaultCacheConfig.Configuration.OnResult = null;
        }
        
        [Fact]
        public async Task DefaultOnFetchIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new List<FunctionCacheFetchResult>();

            DefaultCacheConfig.Configuration.OnFetch = x =>
            {
                if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                    results.Add(x);
            };
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }

            DefaultCacheConfig.Configuration.OnFetch = null;
        }
        
        [Fact]
        public async Task DefaultOnErrorIsTriggered()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var errors = new List<FunctionCacheErrorEvent>();

            DefaultCacheConfig.Configuration.OnError = x =>
            {
                if (x.Keys.FirstOrDefault()?.StartsWith(KeyPrefix) ?? false)
                    errors.Add(x);
            };
            
            var cachedEcho = echo
                .Cached()
                .Build();

            var previousErrorCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var key = GetRandomKey();
                
                if (i % 2 == 0)
                {
                    await Assert.ThrowsAsync<Exception>(() => cachedEcho(key));
                    Assert.Equal(previousErrorCount += 2, errors.Count); // one for failing the fetch, one for failing the get
                    Assert.Equal(key, errors[errors.Count - 1].Keys.Single());
                    Assert.Equal(key, errors[errors.Count - 2].Keys.Single());
                }
                else
                {
                    Assert.Equal(key, await cachedEcho(key));
                }
            }
            
            DefaultCacheConfig.Configuration.OnError = null;
        }

        private static string GetRandomKey()
        {
            return KeyPrefix + Guid.NewGuid();
        }
    }
}