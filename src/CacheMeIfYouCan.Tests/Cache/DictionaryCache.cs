using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class DictionaryCache
    {
        private readonly CacheSetupLock _setupLock;

        public DictionaryCache(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task KeysAreRemovedAutomaticallyOnceTheyExpire()
        {
            Func<string, Task<string>> echo = new Echo();

            var cache = new DictionaryCache<string, string>("echo");
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromTicks(1))
                    .WithLocalCache(cache)
                    .Build();
            }

            await cachedEcho("123");

            cache.Count.Should().Be(1);

            await Task.Delay(TimeSpan.FromSeconds(22));
            
            cache.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task KeysThatHaveNotExpiredAreNotRemoved()
        {
            Func<string, Task<string>> echo = new Echo();

            var cache = new DictionaryCache<string, string>("echo");
            
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromMinutes(1))
                    .WithLocalCache(cache)
                    .Build();
            }

            await cachedEcho("123");
            
            cache.Count.Should().Be(1);

            await Task.Delay(TimeSpan.FromSeconds(11));
            
            cache.Count.Should().Be(1);
        }
        
        [Fact]
        public void CountIsCorrect()
        {
            var cache = new DictionaryCache<int, Guid>("test");
            
            for (var i = 0; i < 1000; i++)
                cache.Set(new Key<int>(i, i.ToString()), Guid.NewGuid(), TimeSpan.FromMinutes(1));

            cache.Count.Should().Be(1000);
        }

        [Fact]
        public async Task MaxItemsIsHonoured()
        {
            var cache = new DictionaryCache<int, Guid>("test", 1000);
            
            for (var i = 0; i < 2000; i++)
                cache.Set(new Key<int>(i, i.ToString()), Guid.NewGuid(), TimeSpan.FromMinutes(1));

            await Task.Delay(TimeSpan.FromSeconds(15));
            
            cache.Count.Should().Be(1000);
        }
    }
}