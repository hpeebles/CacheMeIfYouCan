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
                .Configure(c => c.OnGetResultObservable(x => x.Subscribe(results.Add)))
                .Build(new DistributedCacheFactoryConfig<string, string>());

            await cache.Get(new Key<string>("123", "123"));

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnSetResult()
        {
            var results = new List<CacheSetResult>();

            var cache = new TestCacheFactory()
                .Configure(c => c.OnSetResultObservable(x => x.Subscribe(results.Add)))
                .Build(new DistributedCacheFactoryConfig<string, string>());

            await cache.Set(new Key<string>("123", "123"), "123", TimeSpan.FromMinutes(1));

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnError()
        {
            var errors = new List<CacheException>();

            var cache = new TestCacheFactory(error: () => true)
                .Configure(c => c.OnErrorObservable(x => x.Subscribe(errors.Add)))
                .Build(new DistributedCacheFactoryConfig<string, string>());

            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(new Key<string>("123", "123")));

            Assert.Single(errors);
        }
    }
}