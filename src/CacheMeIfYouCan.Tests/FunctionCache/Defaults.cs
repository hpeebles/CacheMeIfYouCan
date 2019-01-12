using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class Defaults
    {
        private readonly CacheSetupLock _setupLock;

        public Defaults(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        private const string KeyPrefix = "DefaultsTests";
        
        [Fact]
        public async Task DefaultOnResultIsTriggered()
        {
            var results = new List<FunctionCacheGetResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnResultAction(x =>
                {
                    if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                        results.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnResultAction(null, AdditionBehaviour.Overwrite);
            }

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }
        }
        
        [Fact]
        public async Task DefaultOnFetchIsTriggered()
        {
            var results = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnFetchAction(x =>
                {
                    if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                        results.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnFetchAction(null, AdditionBehaviour.Overwrite);
            }

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }
        }
        
        [Fact]
        public async Task DefaultOnExceptionIsTriggered()
        {
            var errors = new List<FunctionCacheException>();

            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnExceptionAction(x =>
                {
                    if (x.Keys.FirstOrDefault()?.StartsWith(KeyPrefix) ?? false)
                        errors.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnExceptionAction(null, AdditionBehaviour.Overwrite);
            }

            var previousErrorCount = 0;
            for (var i = 0; i < 10; i++)
            {
                var key = GetRandomKey();
                
                if (i % 2 == 0)
                {
                    await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(key));
                    Assert.Equal(previousErrorCount += 2, errors.Count); // one for failing the fetch, one for failing the get
                    Assert.Equal(key, errors[errors.Count - 1].Keys.Single());
                    Assert.Equal(key, errors[errors.Count - 2].Keys.Single());
                }
                else
                {
                    Assert.Equal(key, await cachedEcho(key));
                }
            }
        }
        
        [Fact]
        public async Task DefaultOnCacheGetIsTriggered()
        {
            var results = new List<CacheGetResult>();

            var key = Guid.NewGuid().ToString();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnCacheGetAction(x =>
                {
                    if (x.Hits.Contains(key) || x.Misses.Contains(key))
                        results.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnCacheGetAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(results);
            Assert.True(results.Single().Success);
            Assert.Empty(results.Single().Hits);
            Assert.Single(results.Single().Misses);
            Assert.Equal(key, results.Single().Misses.Single());
            Assert.Equal("memory", results.Single().CacheType);
            
            for (var i = 2; i < 10; i++)
            {
                await cachedEcho(key);
                
                Assert.Equal(i, results.Count);
                Assert.True(results.Last().Success);
                Assert.Single(results.Last().Hits);
                Assert.Empty(results.Last().Misses);
                Assert.Equal(key, results.Last().Hits.Single());
                Assert.Equal("memory", results.Last().CacheType);
            }
        }
        
        [Fact]
        public async Task DefaultOnCacheSetIsTriggered()
        {
            var results = new List<CacheSetResult>();

            var key = Guid.NewGuid().ToString();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnCacheSetAction(x =>
                {
                    if (x.Keys.Contains(key))
                        results.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .Build();

                DefaultSettings.Cache.WithOnCacheSetAction(null, AdditionBehaviour.Overwrite);
            }

            await cachedEcho(key);

            Assert.Single(results);
            Assert.True(results.Single().Success);
            Assert.Single(results.Single().Keys);
            Assert.Equal(key, results.Single().Keys.Single());
            Assert.Equal("memory", results.Single().CacheType);
            
            await cachedEcho(key);
            
            Assert.Single(results);
        }

        [Fact]
        public async Task DefaultOnCacheExceptionIsTriggered()
        {
            var errors = new List<CacheException>();

            var key = Guid.NewGuid().ToString();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.WithOnCacheExceptionAction(x =>
                {
                    if (x.Keys.Contains(key))
                        errors.Add(x);
                });

                cachedEcho = echo
                    .Cached()
                    .WithDistributedCache(new TestCache<string, string>(x => x, x => x, error: () => true))
                    .Build();

                DefaultSettings.Cache.WithOnExceptionAction(null, AdditionBehaviour.Overwrite);
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(key));

            Assert.Single(errors);
            Assert.Single(errors.Single().Keys);
            Assert.Equal(key, errors.Single().Keys.Single());
            Assert.Equal("test", errors.Single().CacheType);
        }
        
        private static string GetRandomKey()
        {
            return KeyPrefix + Guid.NewGuid();
        }
    }
}