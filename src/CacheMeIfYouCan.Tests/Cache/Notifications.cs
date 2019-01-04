using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Cache.Helpers;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class Notifications : CacheTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExceptionsCanBeFilteredForDistributedCache(bool filterSucceeds)
        {
            var errors = new List<CacheException>();

            IDistributedCache<string, string> cache;
            using (EnterSetup(false))
            {
                cache = new TestCacheFactory(error: () => true)
                    .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                    .OnException(ex => ex.InnerException is TestException == filterSucceeds, errors.Add)
                    .Build<string, string>("test");
            }

            await Assert.ThrowsAsync<CacheGetException<string>>(() => cache.Get(new Key<string>("abc", "abc")));

            if (filterSucceeds)
            {
                Assert.Single(errors);
                Assert.Equal("abc", errors[0].Keys.Single());
            }
            else
            {
                Assert.Empty(errors);
            }
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionsCanBeFilteredForLocalCache(bool filterSucceeds)
        {
            var errors = new List<CacheException>();

            ILocalCache<string, string> cache;
            using (EnterSetup(false))
            {
                cache = new TestLocalCacheFactory(error: () => true)
                    .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                    .OnException(ex => ex.InnerException is TestException == filterSucceeds, errors.Add)
                    .Build<string, string>("test");
            }

            Assert.Throws<CacheGetException<string>>(() => cache.Get(new Key<string>("abc", "abc")));

            if (filterSucceeds)
            {
                Assert.Single(errors);
                Assert.Equal("abc", errors[0].Keys.Single());
            }
            else
            {
                Assert.Empty(errors);
            }
        }
    }
}