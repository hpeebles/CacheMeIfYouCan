using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Cache.Helpers;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class Notifications
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExceptionsCanBeFilteredForDistributedCache(bool filterSucceeds)
        {
            var errors = new List<CacheException>();
            
            var cache = new TestCacheFactory(error: () => true)
                .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                .OnError(ex => ex.InnerException is CrazyException == filterSucceeds, errors.Add)
                .Build<string, string>("test");

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
            
            var cache = new TestLocalCacheFactory(error: () => true)
                .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                .OnError(ex => ex.InnerException is CrazyException == filterSucceeds, errors.Add)
                .Build<string, string>("test");

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