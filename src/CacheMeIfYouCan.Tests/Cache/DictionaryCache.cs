using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class DictionaryCache
    {
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