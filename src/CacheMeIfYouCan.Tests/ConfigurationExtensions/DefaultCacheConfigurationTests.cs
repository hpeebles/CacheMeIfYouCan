using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class DefaultCacheConfigurationTests : CacheTestBase
    {
        [Fact]
        public async Task OnResult()
        {
            var results = new List<FunctionCacheGetResult>();
            
            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnResultObservable(x => x
                    .Where(r => r.Results.Any(g => g.KeyString == key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnResultAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnFetch()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnFetchObservable(x => x
                    .Where(r => r.Results.Any(f => f.KeyString == key))
                    .Subscribe(fetches.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnFetchAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(fetches);
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<FunctionCacheException>();
            
            var key = Guid.NewGuid().ToString();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnExceptionObservable(x => x
                    .Where(r => r.Keys.Contains(key))
                    .Subscribe(errors.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnExceptionAction(null, AdditionBehaviour.Overwrite);
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(key));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            var results = new List<CacheGetResult>();

            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnCacheGetObservable(x => x
                    .Where(r => r.Hits.Concat(r.Misses).Contains(key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnCacheGetAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            var results = new List<CacheSetResult>();

            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnCacheSetObservable(x => x
                    .Where(r => r.Keys.Contains(key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnCacheSetAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheException()
        {
            var errors = new List<CacheException>();

            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(true))
            {
                DefaultSettings.Cache.WithOnCacheExceptionObservable(x => x.Subscribe(errors.Add));

                var cacheFactory = new TestCacheFactory(error: () => true);

                cachedEcho = echo
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory)
                    .Build();

                DefaultSettings.Cache.WithOnCacheExceptionAction(null, AdditionBehaviour.Overwrite);
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Single(errors);
        }
    }
}