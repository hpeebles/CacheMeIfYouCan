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
    public class DefaultCacheConfigurationTests
    {
        [Fact]
        public async Task OnResult()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, Task<string>> cachedEcho;
            
            var key = Guid.NewGuid().ToString();
            
            lock (DefaultSettingsLock.Lock)
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
            Func<string, Task<string>> echo = new Echo();
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, Task<string>> cachedEcho;

            var key = Guid.NewGuid().ToString();
            
            lock (DefaultSettingsLock.Lock)
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
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<FunctionCacheException>();
            
            Func<string, Task<string>> cachedEcho;

            var key = Guid.NewGuid().ToString();
            
            lock (DefaultSettingsLock.Lock)
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
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheGetResult>();

            Func<string, Task<string>> cachedEcho;

            var key = Guid.NewGuid().ToString();
            
            lock (DefaultSettingsLock.Lock)
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
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheSetResult>();

            Func<string, Task<string>> cachedEcho;

            var key = Guid.NewGuid().ToString();
            
            lock (DefaultSettingsLock.Lock)
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
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => true);
            
            var errors = new List<CacheException>();

            Func<string, Task<string>> cachedEcho;

            lock (DefaultSettingsLock.Lock)
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