using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.DictionaryCache
{
    public class KeyRemoval
    {
        [Fact]
        public async Task KeysAreRemovedAutomaticallyOnceTheyExpire()
        {
            Func<string, Task<string>> echo = new Echo();

            var cache = new DictionaryCache<string, string>("echo");
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromTicks(1))
                .WithLocalCache(cache)
                .Build();

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
            
            var cachedEcho = echo
                .Cached()
                .For(TimeSpan.FromMinutes(1))
                .WithLocalCache(cache)
                .Build();

            await cachedEcho("123");
            
            Assert.Equal(1, cache.Count);

            await Task.Delay(TimeSpan.FromSeconds(11));
            
            Assert.Equal(1, cache.Count);
        }
    }
}