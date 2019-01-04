using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class DictionaryCache : CacheTestBase
    {
        [Fact]
        public async Task KeysAreRemovedAutomaticallyOnceTheyExpire()
        {
            Func<string, Task<string>> echo = new Echo();

            var cache = new DictionaryCache<string, string>("echo");
            
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromTicks(1))
                    .WithLocalCache(cache)
                    .Build();
            }

            await cachedEcho("123");
            
            Assert.Equal(1, cache.Count);

            await Task.Delay(TimeSpan.FromSeconds(11));
            
            Assert.Equal(0, cache.Count);
        }
        
        [Fact]
        public async Task KeysThatHaveNotExpiredAreNotRemoved()
        {
            Func<string, Task<string>> echo = new Echo();

            var cache = new DictionaryCache<string, string>("echo");
            
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMinutes(1))
                    .WithLocalCache(cache)
                    .Build();
            }

            await cachedEcho("123");
            
            Assert.Equal(1, cache.Count);

            await Task.Delay(TimeSpan.FromSeconds(11));
            
            Assert.Equal(1, cache.Count);
        }
        
        [Fact]
        public void CountIsCorrect()
        {
            var cache = new DictionaryCache<int, Guid>("test");
            
            for (var i = 0; i < 1000; i++)
                cache.Set(new Key<int>(i, i.ToString()), Guid.NewGuid(), TimeSpan.FromMinutes(1));

            Assert.Equal(1000, cache.Count);
        }

        [Fact]
        public async Task MaxItemsIsHonoured()
        {
            var cache = new DictionaryCache<int, Guid>("test", 1000);
            
            for (var i = 0; i < 2000; i++)
                cache.Set(new Key<int>(i, i.ToString()), Guid.NewGuid(), TimeSpan.FromMinutes(1));

            await Task.Delay(TimeSpan.FromSeconds(15));
            
            Assert.Equal(1000, cache.Count);
        }
    }
}