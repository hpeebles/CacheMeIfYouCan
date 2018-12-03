using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests;
using Xunit;

namespace CacheMeIfYouCan.Observables.Tests
{
    public class DefaultCacheConfigurationTests
    {
        [Fact]
        public async Task OnResult()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();

            DefaultCacheConfig.Configuration.WithOnResultObservable(x => x.Subscribe(results.Add));
            
            var cachedEcho = echo
                .Cached()
                .Build();

            DefaultCacheConfig.Configuration.WithOnResultAction(null, ActionOrdering.Overwrite);

            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnFetch()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            DefaultCacheConfig.Configuration.WithOnFetchObservable(x => x.Subscribe(fetches.Add));
            
            var cachedEcho = echo
                .Cached()
                .Build();

            DefaultCacheConfig.Configuration.WithOnFetchAction(null, ActionOrdering.Overwrite);
            
            await cachedEcho("123");

            Assert.Single(fetches);
        }
        
        [Fact]
        public async Task OnError()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<FunctionCacheException>();
            
            DefaultCacheConfig.Configuration.WithOnErrorObservable(x => x.Subscribe(errors.Add));

            var cachedEcho = echo
                .Cached()
                .Build();

            DefaultCacheConfig.Configuration.WithOnErrorAction(null, ActionOrdering.Overwrite);
            
            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheGetResult>();

            DefaultCacheConfig.Configuration.WithOnCacheGetObservable(x => x.Subscribe(results.Add));
            
            var cachedEcho = echo
                .Cached()
                .Build();

            DefaultCacheConfig.Configuration.WithOnCacheGetAction(null, ActionOrdering.Overwrite);
            
            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheSetResult>();

            DefaultCacheConfig.Configuration.WithOnCacheSetObservable(x => x.Subscribe(results.Add));
            
            var cachedEcho = echo
                .Cached()
                .Build();

            DefaultCacheConfig.Configuration.WithOnCacheSetAction(null, ActionOrdering.Overwrite);
            
            await cachedEcho("123");

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheError()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<CacheException>();

            DefaultCacheConfig.Configuration.WithOnCacheErrorObservable(x => x.Subscribe(errors.Add));
            
            var cache = new TestCacheFactory(error: () => true)
                .Configure(x => { })
                .Build(new DistributedCacheFactoryConfig<string, string>());
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(cache)
                .Build();

            DefaultCacheConfig.Configuration.WithOnCacheErrorAction(null, ActionOrdering.Overwrite);
            
            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Single(errors);
        }
    }
}