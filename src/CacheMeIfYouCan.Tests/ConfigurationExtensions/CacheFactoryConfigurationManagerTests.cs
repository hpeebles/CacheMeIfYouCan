using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    [Collection(TestCollections.Cache)]
    public class CacheFactoryConfigurationManagerTests
    {
        private readonly CacheSetupLock _setupLock;

        public CacheFactoryConfigurationManagerTests(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task OnGetResult()
        {
            var results = new List<CacheGetResult>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory()
                    .OnGetResultObservable(x => x.Subscribe(results.Add))
                    .Build<string, string>("test");
            }

            await cache.Get(new Key<string>("123", "123"));

            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnSetResult()
        {
            var results = new List<CacheSetResult>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory()
                    .OnSetResultObservable(x => x.Subscribe(results.Add))
                    .Build<string, string>("test");
            }

            await cache.Set(new Key<string>("123", "123"), "123", TimeSpan.FromMinutes(1));

            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<CacheException>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(error: () => true)
                    .OnExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build<string, string>("test");
            }

            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(new Key<string>("123", "123")));

            errors.Should().ContainSingle();
        }
    }
}