using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class CacheFactoryTests
    {
        private readonly CacheSetupLock _setupLock;

        public CacheFactoryTests(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task OnGetResult()
        {
            var results = new List<CacheGetResult>();

            ICache<string, int> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory()
                    .OnGetResult(results.Add)
                    .BuildAsCache<string, int>("abc");
            }

            const string key = "abc";
            
            await cache.Get(key);
            await cache.Set(key, 123, TimeSpan.FromSeconds(1));
            await cache.Get(key);
            
            results.Count.Should().Be(2);
            results[0].Success.Should().BeTrue();
            results[0].Misses.Should().ContainSingle();
            results[0].Misses[0].Should().Be(key);
            results[1].Success.Should().BeTrue();
            results[1].Hits.Should().ContainSingle();
            results[1].Hits[0].Should().Be(key);
        }
        
        [Fact]
        public async Task OnSetResult()
        {
            var results = new List<CacheSetResult>();
            
            ICache<string, int> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory()
                    .OnSetResult(results.Add)
                    .BuildAsCache<string, int>("abc");
            }

            const string key = "abc";
            
            await cache.Set(key, 123, TimeSpan.FromSeconds(1));
            
            results.Should().ContainSingle();
            results[0].Success.Should().BeTrue();
            results[0].Keys.Should().ContainSingle();
            results[0].Keys[0].Should().Be(key);
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<CacheException>();

            ICache<string, int> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(error: () => true)
                    .OnException(errors.Add)
                    .BuildAsCache<string, int>("abc");
            }

            const string key = "abc";
            
            Func<Task<int>> func = () => cache.Get(key);
            func.Should().Throw<CacheException>();
            
            errors.Should().ContainSingle();
            errors[0].Keys.Should().ContainSingle();
            errors[0].Keys.Single().Should().Be(key);
        }
        
        [Fact]
        public async Task BuildAsCacheExtension()
        {
            ICache<string, int> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory()
                    .BuildAsCache<string, int>("abc");
            }

            await cache.Set("abc", 123, TimeSpan.FromMilliseconds(100));
            
            (await cache.Get("abc")).Should().Be(123);

            await Task.Delay(TimeSpan.FromMilliseconds(200));
            
            (await cache.Get("abc")).Should().Be(0);
        }
        
        [Theory]
        [InlineData("memory")]
        [InlineData("dictionary")]
        public async Task CachesDontShareValues(string cacheType)
        {
            ICache<int, int> cache1;
            ICache<int, int> cache2;
            using (_setupLock.Enter())
            {
                ILocalCacheFactory cacheFactory;
                if (cacheType == "memory")
                    cacheFactory = new MemoryCacheFactory();
                else if (cacheType == "dictionary")
                    cacheFactory = new DictionaryCacheFactory();
                else
                    throw new Exception($"CacheType not recognised '{cacheType}'");

                cache1 = cacheFactory.BuildAsCache<int, int>("1");
                cache2 = cacheFactory.BuildAsCache<int, int>("2");
            }

            await cache1.Set(123, 123, TimeSpan.FromSeconds(1));
            
            (await cache1.Get(123)).Should().Be(123);
            (await cache2.Get(123)).Should().Be(0);
        }
        
        [Fact]
        public async Task SetMultipleSerializers()
        {
            var serializer1 = new TestSerializer();
            var serializer2 = new TestSerializer();
            var serializer3 = new TestSerializer();

            ICache<int, int> cacheInt;
            ICache<long, long> cacheLong;
            ICache<string, string> cacheString;
            using (_setupLock.Enter())
            {
                var cacheFactory = new TestCacheFactory()
                    .WithKeySerializers(c => c
                        .Set<int>(serializer1)
                        .Set<long>(serializer2)
                        .SetDefault(serializer3));

                cacheInt = cacheFactory.BuildAsCache<int, int>("int");
                cacheLong = cacheFactory.BuildAsCache<long, long>("long");
                cacheString = cacheFactory.BuildAsCache<string, string>("string");
            }

            await cacheInt.Set(123, 123, TimeSpan.FromSeconds(1));

            serializer1.SerializeCount.Should().Be(1);
            serializer2.SerializeCount.Should().Be(0);
            serializer3.SerializeCount.Should().Be(0);

            await cacheLong.Set(123, 123, TimeSpan.FromSeconds(1));
            
            serializer1.SerializeCount.Should().Be(1);
            serializer2.SerializeCount.Should().Be(1);
            serializer3.SerializeCount.Should().Be(0);
            
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            await cacheString.Set("123", "123", TimeSpan.FromSeconds(1));
            
            serializer1.SerializeCount.Should().Be(1);
            serializer2.SerializeCount.Should().Be(1);
            serializer3.SerializeCount.Should().Be(1);
        }
    }
}