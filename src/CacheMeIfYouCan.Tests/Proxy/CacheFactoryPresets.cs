using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.FunctionCache;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    public class CacheFactoryPresets
    {
        [Fact]
        public async Task ValidIdSucceeds()
        {
            var results = new List<CacheGetResult>();
            
            var cacheFactory = new TestCacheFactory()
                .OnGetResult(results.Add);

            var key = Guid.NewGuid().ToString();

            DefaultSettings.Cache.CreateCacheFactoryPreset(21, cacheFactory);

            ITest impl = new TestImpl();

            var proxy = impl
                .Cached()
                .WithCacheFactoryPreset(21)
                .Build();

            await proxy.StringToString(key);

            Assert.Single(results);
            Assert.Single(results[0].Misses);
            Assert.Equal(key, results[0].Misses[0]);
        }
        
        [Fact]
        public async Task ValidEnumSucceeds()
        {
            var results = new List<CacheGetResult>();
            
            var cacheFactory = new TestCacheFactory()
                .OnGetResult(results.Add);

            var key = Guid.NewGuid().ToString();

            DefaultSettings.Cache.CreateCacheFactoryPreset(TestEnum.TwentyTwo, cacheFactory);
            
            ITest impl = new TestImpl();

            var proxy = impl
                .Cached()
                .WithCacheFactoryPreset(TestEnum.TwentyTwo)
                .Build();

            await proxy.StringToString(key);

            Assert.Single(results);
            Assert.Single(results[0].Misses);
            Assert.Equal(key, results[0].Misses[0]);
        }

        [Fact]
        public void InvalidIdFails()
        {
            ITest impl = new TestImpl();

            Assert.Throws<Exception>(() => impl
                .Cached()
                .WithCacheFactoryPreset(1000)
                .Build());
        }
        
        [Theory]
        [InlineData(true, 23, TestEnum.TwentyThree)]
        [InlineData(false, 24, TestEnum.TwentyFour)]
        public async Task IntAndEnumRemainSeparate(bool useInt, int intValue, TestEnum enumValue)
        {
            var resultsInt = new List<CacheGetResult>();
            var resultsEnum = new List<CacheGetResult>();
            
            var cacheFactoryInt = new TestCacheFactory().OnGetResult(resultsInt.Add);
            var cacheFactoryEnum = new TestCacheFactory().OnGetResult(resultsEnum.Add);

            var key = Guid.NewGuid().ToString();

            DefaultSettings.Cache.CreateCacheFactoryPreset(intValue, cacheFactoryInt);
            DefaultSettings.Cache.CreateCacheFactoryPreset(enumValue, cacheFactoryEnum);

            ITest impl = new TestImpl();
            ITest proxy;
            if (useInt)
            {
                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(intValue)
                    .Build();
            }
            else
            {
                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(enumValue)
                    .Build();
            }

            await proxy.StringToString(key);

            var expectedWithResults = useInt ? resultsInt : resultsEnum;
            var expectedWithoutResults = useInt ? resultsEnum : resultsInt;
            
            Assert.Single(expectedWithResults);
            Assert.Single(expectedWithResults[0].Misses);
            Assert.Equal(key, expectedWithResults[0].Misses[0]);
            Assert.Empty(expectedWithoutResults);
        }

        [Fact]
        public async Task DefaultLocalCacheRemovedIfNoneDefinedInPreset()
        {
            var shouldBePopulated = new List<CacheGetResult>();
            var shouldBeEmpty = new List<CacheGetResult>();
            
            var shouldRemain = new TestCacheFactory().OnGetResult(shouldBePopulated.Add);
            var shouldBeRemoved = new TestLocalCacheFactory().OnGetResult(shouldBeEmpty.Add);

            DefaultSettings.Cache.CreateCacheFactoryPreset(25, shouldRemain);
            
            ITest impl = new TestImpl();
            ITest proxy;
            lock (DefaultSettingsLock.Lock)
            {
                DefaultSettings.Cache.WithLocalCacheFactory(shouldBeRemoved);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(25)
                    .Build();

                DefaultSettings.Cache.WithLocalCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await proxy.StringToString(key);
            
            Assert.Single(shouldBePopulated);
            Assert.Single(shouldBePopulated[0].Misses);
            Assert.Equal(key, shouldBePopulated[0].Misses[0]);
            Assert.Empty(shouldBeEmpty);
        }
        
        [Fact]
        public async Task DefaultDistributedCacheRemovedIfNoneDefinedInPreset()
        {
            var shouldBePopulated = new List<CacheGetResult>();
            var shouldBeEmpty = new List<CacheGetResult>();
            
            var shouldRemain = new TestLocalCacheFactory().OnGetResult(shouldBePopulated.Add);
            var shouldBeRemoved = new TestCacheFactory().OnGetResult(shouldBeEmpty.Add);

            DefaultSettings.Cache.CreateCacheFactoryPreset(26, shouldRemain);
            
            ITest impl = new TestImpl();
            ITest proxy;
            lock (DefaultSettingsLock.Lock)
            {
                DefaultSettings.Cache.WithDistributedCacheFactory(shouldBeRemoved);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(26)
                    .Build();

                DefaultSettings.Cache.WithDistributedCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await proxy.StringToString(key);
            
            Assert.Single(shouldBePopulated);
            Assert.Single(shouldBePopulated[0].Misses);
            Assert.Equal(key, shouldBePopulated[0].Misses[0]);
            Assert.Empty(shouldBeEmpty);
        }
    }
}