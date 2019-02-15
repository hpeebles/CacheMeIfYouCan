using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
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
            var results = new ConcurrentBag<CacheGetResult>();

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

            results
                .Where(r => !r.StatusCodeCounts.Any())
                .Should()
                .ContainSingle();

            foreach (var result in results.Where(r => r.StatusCodeCounts.Any()))
            {
                result.StatusCodeCounts.Should().ContainSingle();
                result.StatusCodeCounts.Single().StatusCode.Should().Be(DuplicateStatusCode);
                result.StatusCodeCounts.Single().Count.Should().Be(1);
            }
        }
        
        [Fact]
        public async Task DuplicatesAreCaught_Multi()
        {
            var results = new ConcurrentBag<CacheGetResult>();

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

            results
                .Where(r => !r.StatusCodeCounts.Any())
                .Should()
                .ContainSingle();

            foreach (var result in results.Where(r => r.StatusCodeCounts.Any()))
            {
                result.StatusCodeCounts.Should().ContainSingle();
                result.StatusCodeCounts.Single().StatusCode.Should().Be(DuplicateStatusCode);
                result.StatusCodeCounts.Single().Count.Should().Be(1);
            }
        }
    } 
}