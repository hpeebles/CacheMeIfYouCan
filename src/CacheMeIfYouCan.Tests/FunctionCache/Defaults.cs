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

            DefaultCacheConfig.Configuration.WithOnResultAction(x =>
            {
                if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                    results.Add(x);
            });
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }
            
            DefaultCacheConfig.Configuration.WithOnResultAction(null);
        }
        
        [Fact]
        public async Task DefaultOnFetchIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero);

            var results = new List<FunctionCacheFetchResult>();

            DefaultCacheConfig.Configuration.WithOnFetchAction(x =>
            {
                if (x.Results.FirstOrDefault()?.KeyString.StartsWith(KeyPrefix) ?? false)
                    results.Add(x);
            });
            
            var cachedEcho = echo
                .Cached()
                .Build();

            for (var i = 1; i < 10; i++)
            {
                var key = GetRandomKey();

                await cachedEcho(key);
                Assert.Equal(i, results.Count);
            }

            DefaultCacheConfig.Configuration.WithOnFetchAction(null);
        }
        
        [Fact]
        public async Task DefaultOnErrorIsTriggered()
        {
            var count = 0;
            
            Func<string, Task<string>> echo = new Echo(TimeSpan.Zero, x => count++ % 2 == 0);

            var errors = new List<FunctionCacheErrorEvent>();

            DefaultCacheConfig.Configuration.WithOnErrorAction(x =>
            {
                if (x.Keys.FirstOrDefault()?.StartsWith(KeyPrefix) ?? false)
                    errors.Add(x);
            });
            
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
            
            DefaultCacheConfig.Configuration.WithOnErrorAction(null);
        }
        
        [Fact]
        public async Task DefaultOnCacheGetIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo();

            var results = new List<CacheGetResult>();

            var key = Guid.NewGuid().ToString();
            
            DefaultCacheConfig.Configuration.WithOnCacheGetAction(x =>
            {
                if (x.Hits.Contains(key) || x.Misses.Contains(key))
                    results.Add(x);
            });
            
            var cachedEcho = echo
                .Cached()
                .Build();

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
            
            DefaultCacheConfig.Configuration.WithOnCacheGetAction(null);
        }
        
        [Fact]
        public async Task DefaultOnCacheSetIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo();

            var results = new List<CacheSetResult>();

            var key = Guid.NewGuid().ToString();
            
            DefaultCacheConfig.Configuration.WithOnCacheSetAction(x =>
            {
                if (x.Keys.Contains(key))
                    results.Add(x);
            });
            
            var cachedEcho = echo
                .Cached()
                .Build();

            await cachedEcho(key);

            Assert.Single(results);
            Assert.True(results.Single().Success);
            Assert.Single(results.Single().Keys);
            Assert.Equal(key, results.Single().Keys.Single());
            Assert.Equal("memory", results.Single().CacheType);
            
            await cachedEcho(key);
            
            Assert.Single(results);
            
            DefaultCacheConfig.Configuration.WithOnCacheSetAction(null);
        }

        [Fact]
        public async Task DefaultOnCacheErrorIsTriggered()
        {
            Func<string, Task<string>> echo = new Echo();

            var errors = new List<CacheErrorEvent>();

            var key = Guid.NewGuid().ToString();
            
            DefaultCacheConfig.Configuration.WithOnCacheErrorAction(x =>
            {
                if (x.Keys.Contains(key))
                    errors.Add(x);
            });
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(new TestCache<string, string>(x => x, x => x, error: () => true))
                .Build();

            await Assert.ThrowsAsync<Exception>(() => cachedEcho(key));

            Assert.Single(errors);
            Assert.Single(errors.Single().Keys);
            Assert.Equal(key, errors.Single().Keys.Single());
            Assert.Equal("test", errors.Single().CacheType);
            
            DefaultCacheConfig.Configuration.WithOnErrorAction(null);
        }
        
        private static string GetRandomKey()
        {
            return KeyPrefix + Guid.NewGuid();
        }
    }
}