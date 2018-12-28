using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class FunctionCacheTests
    {
        [Fact]
        public async Task OnResult()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnResultObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnFetch()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnFetchObservable(x => x.Subscribe(fetches.Add))
                .Build();

            await cachedEcho("123");

            Assert.Single(fetches);
        }
        
        [Fact]
        public async Task OnException()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<FunctionCacheException>();
            
            var cachedEcho = echo
                .Cached()
                .OnExceptionObservable(x => x.Subscribe(errors.Add))
                .Build();

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnCacheGetObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheSetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnCacheSetObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheException()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<CacheException>();

            var cache = new TestCacheFactory(error: () => true)
                .Build<string, string>("test");
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(cache)
                .OnCacheExceptionObservable(x => x.Subscribe(errors.Add))
                .Build();

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Single(errors);
        }
    }
}