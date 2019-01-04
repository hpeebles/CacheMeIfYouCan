using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class FunctionCacheTests : CacheTestBase
    {
        [Fact]
        public async Task OnResult()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .OnResultObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnFetch()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .OnFetchObservable(x => x.Subscribe(fetches.Add))
                    .Build();
            }

            await cachedEcho("123");

            Assert.Single(fetches);
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<FunctionCacheException>();
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .OnExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            var results = new List<CacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .OnCacheGetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            var results = new List<CacheSetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .OnCacheSetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheException()
        {
            var errors = new List<CacheException>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                var cache = new TestCacheFactory(error: () => true)
                    .Build<string, string>("test");

                cachedEcho = echo
                    .Cached()
                    .WithDistributedCache(cache)
                    .OnCacheExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Single(errors);
        }
    }
}