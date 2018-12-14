using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class CacheFactoryTests
    {
        [Fact]
        public async Task OnGetResult()
        {
            var results = new List<CacheGetResult>();
            
            var cache = new TestCacheFactory()
                .OnGetResult(results.Add)
                .BuildAsCache<string, int>("abc");

            const string key = "abc";
            
            await cache.Get(key);
            await cache.Set(key, 123, TimeSpan.FromSeconds(1));
            await cache.Get(key);
            
            Assert.Equal(2, results.Count);
            Assert.True(results[0].Success);
            Assert.Single(results[0].Misses);
            Assert.Equal(key, results[0].Misses[0]);
            Assert.True(results[1].Success);
            Assert.Single(results[1].Hits);
            Assert.Equal(key, results[1].Hits[0]);
        }
        
        [Fact]
        public async Task OnSetResult()
        {
            var results = new List<CacheSetResult>();
            
            var cache = new TestCacheFactory()
                .OnSetResult(results.Add)
                .BuildAsCache<string, int>("abc");

            const string key = "abc";
            
            await cache.Set(key, 123, TimeSpan.FromSeconds(1));
            
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.Single(results[0].Keys);
            Assert.Equal(key, results[0].Keys[0]);
        }
        
        [Fact]
        public async Task OnError()
        {
            var errors = new List<CacheException>();
            
            var cache = new TestCacheFactory(error: () => true)
                .OnError(errors.Add)
                .BuildAsCache<string, int>("abc");

            const string key = "abc";
            
            await Assert.ThrowsAnyAsync<CacheException>(() => cache.Get(key));
            
            Assert.Single(errors);
            Assert.Single(errors[0].Keys);
            Assert.Equal(key, errors[0].Keys.Single());
        }
        
        [Fact]
        public async Task BuildAsCacheExtension()
        {
            var cache = new TestCacheFactory()
                .BuildAsCache<string, int>("abc");

            await cache.Set("abc", 123, TimeSpan.FromSeconds(1));
            
            Assert.Equal(123, await cache.Get("abc"));

            await Task.Delay(TimeSpan.FromSeconds(2));
            
            Assert.Equal(default, await cache.Get("abc"));
        }
        
        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task CachesDontShareValues(string cacheType)
        {
            ILocalCacheFactory cacheFactory;
            if (cacheType == "memory")
                cacheFactory = new MemoryCacheFactory();
            else if (cacheType == "dictionary")
                cacheFactory = new DictionaryCacheFactory();
            else
                throw new Exception($"CacheType not recognised '{cacheType}'");

            var cache1 = cacheFactory.BuildAsCache<int, int>("1");
            var cache2 = cacheFactory.BuildAsCache<int, int>("2");
            
            await cache1.Set(123, 123, TimeSpan.FromSeconds(1));
            
            Assert.Equal(123, await cache1.Get(123));
            Assert.Equal(default, await cache2.Get(123));
        }
        
        [Fact]
        public async Task SetMultipleSerializers()
        {
            var serializer1 = new TestSerializer();
            var serializer2 = new TestSerializer();
            var serializer3 = new TestSerializer();
            
            var cacheFactory = new TestCacheFactory()
                .WithKeySerializers(c => c
                    .Set<int>(serializer1)
                    .Set<long>(serializer2)
                    .SetDefault(serializer3));

            var cacheInt = cacheFactory.BuildAsCache<int, int>("int");
            var cacheLong = cacheFactory.BuildAsCache<long, long>("long");
            var cacheString = cacheFactory.BuildAsCache<string, string>("string");

            await cacheInt.Set(123, 123, TimeSpan.FromSeconds(1));
            
            Assert.Equal(1, serializer1.SerializeCount);
            Assert.Equal(0, serializer2.SerializeCount);
            Assert.Equal(0, serializer3.SerializeCount);

            await cacheLong.Set(123, 123, TimeSpan.FromSeconds(1));
            
            Assert.Equal(1, serializer1.SerializeCount);
            Assert.Equal(1, serializer2.SerializeCount);
            Assert.Equal(0, serializer3.SerializeCount);
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            await cacheString.Set("123", "123", TimeSpan.FromSeconds(1));
            
            Assert.Equal(1, serializer1.SerializeCount);
            Assert.Equal(1, serializer2.SerializeCount);
            Assert.Equal(1, serializer3.SerializeCount);
        }
    }
}