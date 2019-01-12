using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class DuplicateRequestCatching
    {
        private const int DuplicateStatusCode = 11;
        
        private readonly CacheSetupLock _setupLock;

        public DuplicateRequestCatching(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task DuplicatesAreCaught_Single()
        {
            var results = new List<CacheGetResult>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                    .WithDuplicateRequestCatching()
                    .OnGetResult(results.Add)
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var guid = Guid.NewGuid().ToString();
            var key = new Key<string>(guid, guid);

            await cache.Set(key, guid, TimeSpan.FromMinutes(1));
            
            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => cache.Get(key))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.Single(results.Where(r => !r.StatusCodeCounts.Any()));

            foreach (var result in results.Where(r => r.StatusCodeCounts.Any()))
            {
                Assert.Single(result.StatusCodeCounts);
                Assert.Equal(DuplicateStatusCode, result.StatusCodeCounts.Single().StatusCode);
                Assert.Equal(1, result.StatusCodeCounts.Single().Count);
            }
        }
        
        [Fact]
        public async Task DuplicatesAreCaught_Multi()
        {
            var results = new List<CacheGetResult>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1))
                    .WithDuplicateRequestCatching()
                    .OnGetResult(results.Add)
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var guid = Guid.NewGuid().ToString();
            var key = new Key<string>(guid, guid);

            await cache.Set(key, guid, TimeSpan.FromMinutes(1));
            
            var tasks = Enumerable
                .Range(0, 10)
                .Select(i => i.ToString())
                .Select(i => cache.Get(new[] { key, new Key<string>(i, i) }))
                .ToArray();

            await Task.WhenAll(tasks);

            Assert.Single(results.Where(r => !r.StatusCodeCounts.Any()));

            foreach (var result in results.Where(r => r.StatusCodeCounts.Any()))
            {
                Assert.Single(result.StatusCodeCounts);
                Assert.Equal(DuplicateStatusCode, result.StatusCodeCounts.Single().StatusCode);
                Assert.Equal(1, result.StatusCodeCounts.Single().Count);
            }
        }
    } 
}