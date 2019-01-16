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
    [Collection(TestCollections.Cache)]
    public class DefaultCacheConfigurationTests
    {
        private readonly CacheSetupLock _setupLock;

        public DefaultCacheConfigurationTests(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task OnResult()
        {
            var results = new List<FunctionCacheGetResult>();
            
            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnResultObservable(x => x
                    .Where(r => r.Results.Any(g => g.KeyString == key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.OnResult(null, AdditionBehaviour.Overwrite);
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
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnFetchObservable(x => x
                    .Where(r => r.Results.Any(f => f.KeyString == key))
                    .Subscribe(fetches.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.OnFetch(null, AdditionBehaviour.Overwrite);
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
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnExceptionObservable(x => x
                    .Where(r => r.Keys.Contains(key))
                    .Subscribe(errors.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.OnException(null, AdditionBehaviour.Overwrite);
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
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnCacheGetObservable(x => x
                    .Where(r => r.Hits.Concat(r.Misses).Contains(key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.OnCacheGet(null, AdditionBehaviour.Overwrite);
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
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnCacheSetObservable(x => x
                    .Where(r => r.Keys.Contains(key))
                    .Subscribe(results.Add));

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.OnCacheSet(null, AdditionBehaviour.Overwrite);
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
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.OnCacheExceptionObservable(x => x.Subscribe(errors.Add));

                var cacheFactory = new TestCacheFactory(error: () => true);

                cachedEcho = echo
                    .Cached()
                    .WithDistributedCacheFactory(cacheFactory)
                    .Build();

                DefaultSettings.Cache.OnCacheException(null, AdditionBehaviour.Overwrite);
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));

            Assert.Single(errors);
        }
    }
}