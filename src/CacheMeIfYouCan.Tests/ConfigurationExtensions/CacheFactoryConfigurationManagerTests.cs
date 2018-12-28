using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class CacheFactoryConfigurationManagerTests
    {
        [Fact]
        public async Task OnGetResult()
        {
            var results = new List<CacheGetResult>();

            var cache = new TestCacheFactory()
                .OnGetResultObservable(x => x.Subscribe(results.Add))
                .Build<string, string>("test");

            await cache.Get(new Key<string>("123", "123"));

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnSetResult()
        {
            var results = new List<CacheSetResult>();

            var cache = new TestCacheFactory()
                .OnSetResultObservable(x => x.Subscribe(results.Add))
                .Build<string, string>("test");

            await cache.Set(new Key<string>("123", "123"), "123", TimeSpan.FromMinutes(1));

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<CacheException>();

            var cache = new TestCacheFactory(error: () => true)
                .OnExceptionObservable(x => x.Subscribe(errors.Add))
                .Build<string, string>("test");

            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(new Key<string>("123", "123")));

            Assert.Single(errors);
        }
    }
}