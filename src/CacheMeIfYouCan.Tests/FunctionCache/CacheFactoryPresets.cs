using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
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

            DefaultSettings.Cache.CreateCacheFactoryPreset(1, cacheFactory);

            Func<string, Task<string>> echo = new Echo();

            var cachedEcho = echo
                .Cached()
                .WithCacheFactoryPreset(1)
                .Build();

            await cachedEcho(key);

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

            DefaultSettings.Cache.CreateCacheFactoryPreset(TestEnum.Two, cacheFactory);
            
            Func<string, Task<string>> echo = new Echo();

            var cachedEcho = echo
                .Cached()
                .WithCacheFactoryPreset(TestEnum.Two)
                .Build();

            await cachedEcho(key);

            Assert.Single(results);
            Assert.Single(results[0].Misses);
            Assert.Equal(key, results[0].Misses[0]);
        }

        [Fact]
        public void InvalidIdFails()
        {
            Func<string, Task<string>> echo = new Echo();

            Assert.Throws<Exception>(() => echo
                .Cached()
                .WithCacheFactoryPreset(1000)
                .Build());
        }
        
        [Theory]
        [InlineData(true, 3, TestEnum.Three)]
        [InlineData(false, 4, TestEnum.Four)]
        public async Task IntAndEnumRemainSeparate(bool useInt, int intValue, TestEnum enumValue)
        {
            var resultsInt = new List<CacheGetResult>();
            var resultsEnum = new List<CacheGetResult>();
            
            var cacheFactoryInt = new TestCacheFactory().OnGetResult(resultsInt.Add);
            var cacheFactoryEnum = new TestCacheFactory().OnGetResult(resultsEnum.Add);

            var key = Guid.NewGuid().ToString();

            DefaultSettings.Cache.CreateCacheFactoryPreset(intValue, cacheFactoryInt);
            DefaultSettings.Cache.CreateCacheFactoryPreset(enumValue, cacheFactoryEnum);

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            if (useInt)
            {
                cachedEcho = echo
                    .Cached()
                    .WithCacheFactoryPreset(intValue)
                    .Build();
            }
            else
            {
                cachedEcho = echo
                    .Cached()
                    .WithCacheFactoryPreset(enumValue)
                    .Build();
            }

            await cachedEcho(key);

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

            DefaultSettings.Cache.CreateCacheFactoryPreset(5, shouldRemain);
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            lock (DefaultSettingsLock.Lock)
            {
                DefaultSettings.Cache.WithLocalCacheFactory(shouldBeRemoved);

                cachedEcho = echo
                    .Cached()
                    .WithCacheFactoryPreset(5)
                    .Build();

                DefaultSettings.Cache.WithLocalCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            
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

            DefaultSettings.Cache.CreateCacheFactoryPreset(6, shouldRemain);
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            lock (DefaultSettingsLock.Lock)
            {
                DefaultSettings.Cache.WithDistributedCacheFactory(shouldBeRemoved);

                cachedEcho = echo
                    .Cached()
                    .WithCacheFactoryPreset(6)
                    .Build();

                DefaultSettings.Cache.WithDistributedCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await cachedEcho(key);
            
            Assert.Single(shouldBePopulated);
            Assert.Single(shouldBePopulated[0].Misses);
            Assert.Equal(key, shouldBePopulated[0].Misses[0]);
            Assert.Empty(shouldBeEmpty);
        }
    }
}