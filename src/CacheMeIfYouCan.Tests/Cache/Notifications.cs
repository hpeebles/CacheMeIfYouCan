using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Cache.Helpers;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class Notifications
    {
        private readonly CacheSetupLock _setupLock;

        public Notifications(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExceptionsCanBeFilteredForDistributedCache(bool filterSucceeds)
        {
            var errors = new List<CacheException>();

            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(error: () => true)
                    .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                    .OnException(ex => ex.InnerException is TestException == filterSucceeds, errors.Add)
                    .Build<string, string>("test");
            }

            Func<Task<GetFromCacheResult<string, string>>> func = () => cache.Get(new Key<string>("abc", "abc"));
            await func.Should().ThrowAsync<CacheException<string>>();

            if (filterSucceeds)
            {
                errors.Should().ContainSingle();
                errors[0].Keys.Single().Should().Be("abc");
            }
            else
            {
                errors.Should().BeEmpty();
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionsCanBeFilteredForLocalCache(bool filterSucceeds)
        {
            var errors = new List<CacheException>();

            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(error: () => true)
                    .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                    .OnException(ex => ex.InnerException is TestException == filterSucceeds, errors.Add)
                    .Build<string, string>("test");
            }

            Func<GetFromCacheResult<string, string>> func = () => cache.Get(new Key<string>("abc", "abc"));
            func.Should().Throw<CacheGetException<string>>();

            if (filterSucceeds)
            {
                errors.Should().ContainSingle();
                errors[0].Keys.Single().Should().Be("abc");
            }
            else
            {
                errors.Should().BeEmpty();
            }
        }
    }
}