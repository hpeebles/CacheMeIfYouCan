using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Cache.Helpers;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    public class SwallowingExceptions
    {
        private const string SwallowThis = "swallow this!";
        private const string AlsoSwallowThis = "and this!";
        private const string DontSwallowThis = "but not this!";
        
        [Fact]
        public async Task AllExceptionsForDistributedCache()
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions()
                .Build<string, string>(Guid.NewGuid().ToString());

            var result = await cache.Get(new Key<string>("abc", "abc"));
            
            Assert.False(result.Success);
        }
        
        [Fact]
        public void AllExceptionsForLocalCache()
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions()
                .Build<string, string>(Guid.NewGuid().ToString());

            var result = cache.Get(new Key<string>("abc", "abc"));
            
            Assert.False(result.Success);
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public async Task FilteredByPredicateForDistributedCache(string keyString, bool shouldBeSwallowed)
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());
            
            if (shouldBeSwallowed)
            {
                var result = await cache.Get(new Key<string>(keyString, keyString));
            
                Assert.False(result.Success);
            }
            else
            {
                await Assert.ThrowsAsync<CacheGetException<string>>(() => cache.Get(new Key<string>(keyString, keyString)));
            }
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public void FilteredByPredicateForLocalCache(string keyString, bool shouldBeSwallowed)
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());
            
            if (shouldBeSwallowed)
            {
                var result = cache.Get(new Key<string>(keyString, keyString));
            
                Assert.False(result.Success);
            }
            else
            {
                Assert.Throws<CacheGetException<string>>(() => cache.Get(new Key<string>(keyString, keyString)));
            }
        }

        [Fact]
        public async Task FilteredByTypeForDistributedCache()
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheGetException<string>>()
                .Build<string, string>(Guid.NewGuid().ToString());

            var key = new Key<string>("abc", "abc");
            
            var result = await cache.Get(key);
            
            Assert.False(result.Success);
            
            await Assert.ThrowsAsync<CacheSetException<string, string>>(() => cache.Set(key, "abc", TimeSpan.FromMinutes(1)));
        }
        
        [Fact]
        public void FilteredByTypeForLocalCache()
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheGetException<string>>()
                .Build<string, string>(Guid.NewGuid().ToString());

            var key = new Key<string>("abc", "abc");
            
            var result = cache.Get(key);
            
            Assert.False(result.Success);
            
            Assert.Throws<CacheSetException<string, string>>(() => cache.Set(key, "abc", TimeSpan.FromMinutes(1)));
        }

        [Fact]
        public async Task FilteredByTypeAndPredicateForDistributedCache()
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheGetException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());

            var result = await cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            Assert.False(result.Success);
            
            await Assert.ThrowsAsync<CacheGetException<string>>(() =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis)));
            
            await Assert.ThrowsAsync<CacheSetException<string, string>>(() =>
                cache.Set(new Key<string>(SwallowThis, SwallowThis), "abc", TimeSpan.FromMinutes(1)));
        }
        
        [Fact]
        public void FilteredByTypeAndPredicateForLocalCache()
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheGetException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());

            var result = cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            Assert.False(result.Success);
            
            Assert.Throws<CacheGetException<string>>(() =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis)));
            
            Assert.Throws<CacheSetException<string, string>>(() =>
                cache.Set(new Key<string>(SwallowThis, SwallowThis), "abc", TimeSpan.FromMinutes(1)));
        }
        
        [Fact]
        public async Task CombinedRulesForDistributedCache()
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == AlsoSwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());
            
            var result1 = await cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            Assert.False(result1.Success);
            
            var result2 = await cache.Get(new Key<string>(AlsoSwallowThis, AlsoSwallowThis));
            
            Assert.False(result2.Success);
            
            await Assert.ThrowsAsync<CacheGetException<string>>(() => cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis)));
        }
        
        [Fact]
        public void CombinedRulesForLocalCache()
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == AlsoSwallowThis))
                .Build<string, string>(Guid.NewGuid().ToString());
            
            var result1 = cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            Assert.False(result1.Success);
            
            var result2 = cache.Get(new Key<string>(AlsoSwallowThis, AlsoSwallowThis));
            
            Assert.False(result2.Success);
            
            Assert.Throws<CacheGetException<string>>(() => cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis)));
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public async Task FilteredInnerExceptionsForDistributedCache(string keyString, bool shouldBeSwallowed)
        {
            var cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                .SwallowExceptionsInner(ex => ex.Message == SwallowThis)
                .Build<string, string>(Guid.NewGuid().ToString());
            
            if (shouldBeSwallowed)
            {
                var result = await cache.Get(new Key<string>(keyString, keyString));
            
                Assert.False(result.Success);
            }
            else
            {
                await Assert.ThrowsAsync<CacheGetException<string>>(() => cache.Get(new Key<string>(keyString, keyString)));
            }
        }

        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public void FilteredInnerExceptionsForLocalCache(string keyString, bool shouldBeSwallowed)
        {
            var cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                .SwallowExceptionsInner(ex => ex.Message == SwallowThis)
                .Build<string, string>(Guid.NewGuid().ToString());

            if (shouldBeSwallowed)
            {
                var result = cache.Get(new Key<string>(keyString, keyString));

                Assert.False(result.Success);
            }
            else
            {
                Assert.Throws<CacheGetException<string>>(() => cache.Get(new Key<string>(keyString, keyString)));
            }
        }
    }
}