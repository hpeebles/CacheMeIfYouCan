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
        [Fact]
        public async Task WhenExceptionChangedOnErrorIsStillTriggeredForDistributedCache()
        {
            var errors = new List<CacheException>();
            
            var cache = new TestCacheFactory(error: () => true)
                .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                .OnError(errors.Add)
                .Build<string, string>("test");

            await Assert.ThrowsAsync<CrazyException>(() => cache.Get(new Key<string>("abc", "abc")));

            Assert.Single(errors);
            Assert.Equal("abc", errors[0].Keys.Single());
        }
        
        [Fact]
        public void WhenExceptionChangedOnErrorIsStillTriggeredForLocalCache()
        {
            var errors = new List<CacheException>();
            
            var cache = new TestLocalCacheFactory(error: () => true)
                .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                .OnError(errors.Add)
                .Build<string, string>("test");

            Assert.Throws<CrazyException>(() => cache.Get(new Key<string>("abc", "abc")));

            Assert.Single(errors);
            Assert.Equal("abc", errors[0].Keys.Single());
        }
    }
}