using System;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Cache.Helpers;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Cache
{
    [Collection(TestCollections.Cache)]
    public class SwallowingExceptions
    {
        private const string SwallowThis = "swallow this!";
        private const string AlsoSwallowThis = "and this!";
        private const string DontSwallowThis = "but not this!";
        
        private readonly CacheSetupLock _setupLock;

        public SwallowingExceptions(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task AllExceptionsForDistributedCache()
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions()
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result = await cache.Get(new Key<string>("abc", "abc"));
            
            result.Success.Should().BeFalse();
        }
        
        [Fact]
        public void AllExceptionsForLocalCache()
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions()
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result = cache.Get(new Key<string>("abc", "abc"));
            
            result.Success.Should().BeFalse();
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public async Task FilteredByPredicateForDistributedCache(string keyString, bool shouldBeSwallowed)
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            if (shouldBeSwallowed)
            {
                var result = await cache.Get(new Key<string>(keyString, keyString));
            
                result.Success.Should().BeFalse();
            }
            else
            {
                Func<Task<GetFromCacheResult<string, string>>> func = () =>
                    cache.Get(new Key<string>(keyString, keyString));
                
                await func.Should().ThrowAsync<CacheGetException<string>>();
            }
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public void FilteredByPredicateForLocalCache(string keyString, bool shouldBeSwallowed)
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            if (shouldBeSwallowed)
            {
                var result = cache.Get(new Key<string>(keyString, keyString));
            
                result.Success.Should().BeFalse();
            }
            else
            {
                Func<GetFromCacheResult<string, string>> func = () => cache.Get(new Key<string>(keyString, keyString));
                func.Should().Throw<CacheGetException<string>>();
            }
        }

        [Fact]
        public async Task FilteredByTypeForDistributedCache()
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheGetException<string>>()
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var key = new Key<string>("abc", "abc");
            
            var result = await cache.Get(key);
            
            result.Success.Should().BeFalse();

            Func<Task> func = () => cache.Set(key, "abc", TimeSpan.FromMinutes(1));
            
            await func.Should().ThrowAsync<CacheSetException<string, string>>();
        }
        
        [Fact]
        public void FilteredByTypeForLocalCache()
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheGetException<string>>()
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var key = new Key<string>("abc", "abc");
            
            var result = cache.Get(key);
            
            result.Success.Should().BeFalse();

            Action action = () => cache.Set(key, "abc", TimeSpan.FromMinutes(1));
            
            action.Should().Throw<CacheSetException<string, string>>();
        }

        [Fact]
        public async Task FilteredByTypeAndPredicateForDistributedCache()
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheGetException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result = await cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            result.Success.Should().BeFalse();

            Func<Task<GetFromCacheResult<string, string>>> getFunc = () =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis));

            await getFunc.Should().ThrowAsync<CacheGetException<string>>();

            Func<Task> setFunc = () =>
                cache.Set(new Key<string>(SwallowThis, SwallowThis), "abc", TimeSpan.FromMinutes(1));
            
            await setFunc.Should().ThrowAsync<CacheSetException<string, string>>();
        }
        
        [Fact]
        public void FilteredByTypeAndPredicateForLocalCache()
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheGetException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result = cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            result.Success.Should().BeFalse();

            Func<GetFromCacheResult<string, string>> getFunc = () =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis));
            
            getFunc.Should().Throw<CacheGetException<string>>();

            Action setAction = () =>
                cache.Set(new Key<string>(SwallowThis, SwallowThis), "abc", TimeSpan.FromMinutes(1));
            
            setAction.Should().Throw<CacheSetException<string, string>>();
        }
        
        [Fact]
        public async Task CombinedRulesForDistributedCache()
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == AlsoSwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result1 = await cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            result1.Success.Should().BeFalse();
            
            var result2 = await cache.Get(new Key<string>(AlsoSwallowThis, AlsoSwallowThis));
            
            result2.Success.Should().BeFalse();

            Func<Task<GetFromCacheResult<string, string>>> func = () =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis));
            
            await func.Should().ThrowAsync<CacheGetException<string>>();
        }
        
        [Fact]
        public void CombinedRulesForLocalCache()
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == SwallowThis))
                    .SwallowExceptions<CacheException<string>>(ex => ex.Keys.Any(k => k.AsString == AlsoSwallowThis))
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            var result1 = cache.Get(new Key<string>(SwallowThis, SwallowThis));
            
            result1.Success.Should().BeFalse();
            
            var result2 = cache.Get(new Key<string>(AlsoSwallowThis, AlsoSwallowThis));
            
            result2.Success.Should().BeFalse();

            Func<GetFromCacheResult<string, string>> func = () =>
                cache.Get(new Key<string>(DontSwallowThis, DontSwallowThis));
            
            func.Should().Throw<CacheGetException<string>>();
        }
        
        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public async Task FilteredInnerExceptionsForDistributedCache(string keyString, bool shouldBeSwallowed)
        {
            IDistributedCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .WithWrapper(new DistributedCacheExceptionChangingWrapperFactory())
                    .SwallowExceptionsInner(ex => ex.Message == SwallowThis)
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            if (shouldBeSwallowed)
            {
                var result = await cache.Get(new Key<string>(keyString, keyString));
            
                result.Success.Should().BeFalse();
            }
            else
            {
                Func<Task<GetFromCacheResult<string, string>>> func = () =>
                    cache.Get(new Key<string>(keyString, keyString));
                
                await func.Should().ThrowAsync<CacheGetException<string>>();
            }
        }

        [Theory]
        [InlineData(SwallowThis, true)]
        [InlineData(DontSwallowThis, false)]
        public void FilteredInnerExceptionsForLocalCache(string keyString, bool shouldBeSwallowed)
        {
            ILocalCache<string, string> cache;
            using (_setupLock.Enter())
            {
                cache = new TestLocalCacheFactory(TimeSpan.FromSeconds(1), () => true)
                    .WithWrapper(new LocalCacheExceptionChangingWrapperFactory())
                    .SwallowExceptionsInner(ex => ex.Message == SwallowThis)
                    .Build<string, string>(Guid.NewGuid().ToString());
            }

            if (shouldBeSwallowed)
            {
                var result = cache.Get(new Key<string>(keyString, keyString));

                result.Success.Should().BeFalse();
            }
            else
            {
                Func<GetFromCacheResult<string, string>> func = () => cache.Get(new Key<string>(keyString, keyString));
                
                func.Should().Throw<CacheGetException<string>>();
            }
        }
    }
}