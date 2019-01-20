using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class CacheFactoryPresets
    {
        private readonly CacheSetupLock _setupLock;

        public CacheFactoryPresets(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task ValidIdSucceeds()
        {
            var results = new List<CacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                var cacheFactory = new TestCacheFactory().OnGetResult(results.Add);

                DefaultSettings.Cache.CreateCacheFactoryPreset(21, cacheFactory);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(21)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await proxy.StringToString(key);

            results.Should().ContainSingle();
            results[0].Misses.Should().ContainSingle();
            Assert.Equal(key, results[0].Misses[0]);
        }
        
        [Fact]
        public async Task ValidEnumSucceeds()
        {
            var results = new List<CacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                var cacheFactory = new TestCacheFactory().OnGetResult(results.Add);

                DefaultSettings.Cache.CreateCacheFactoryPreset(TestEnum.TwentyTwo, cacheFactory);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(TestEnum.TwentyTwo)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await proxy.StringToString(key);

            results.Should().ContainSingle();
            results[0].Misses.Should().ContainSingle();
            Assert.Equal(key, results[0].Misses[0]);
        }

        [Fact]
        public void InvalidIdFails()
        {
            ITest impl = new TestImpl();
            using (_setupLock.Enter())
            {
                Assert.Throws<Exception>(() => impl
                    .Cached()
                    .WithCacheFactoryPreset(1000)
                    .Build());
            }
        }
        
        [Theory]
        [InlineData(true, 23, TestEnum.TwentyThree)]
        [InlineData(false, 24, TestEnum.TwentyFour)]
        public async Task IntAndEnumRemainSeparate(bool useInt, int intValue, TestEnum enumValue)
        {
            var resultsInt = new List<CacheGetResult>();
            var resultsEnum = new List<CacheGetResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                var cacheFactoryInt = new TestCacheFactory().OnGetResult(resultsInt.Add);
                var cacheFactoryEnum = new TestCacheFactory().OnGetResult(resultsEnum.Add);

                DefaultSettings.Cache.CreateCacheFactoryPreset(intValue, cacheFactoryInt);
                DefaultSettings.Cache.CreateCacheFactoryPreset(enumValue, cacheFactoryEnum);

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
            }

            var key = Guid.NewGuid().ToString();

            await proxy.StringToString(key);

            var expectedWithResults = useInt ? resultsInt : resultsEnum;
            var expectedWithoutResults = useInt ? resultsEnum : resultsInt;
            
            expectedWithResults.Should().ContainSingle();
            expectedWithResults[0].Misses.Should().ContainSingle();
            Assert.Equal(key, expectedWithResults[0].Misses[0]);
            Assert.Empty(expectedWithoutResults);
        }

        [Fact]
        public async Task DefaultLocalCacheRemovedIfNoneDefinedInPreset()
        {
            var shouldBePopulated = new List<CacheGetResult>();
            var shouldBeEmpty = new List<CacheGetResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                var shouldRemain = new TestCacheFactory().OnGetResult(shouldBePopulated.Add);
                var shouldBeRemoved = new TestLocalCacheFactory().OnGetResult(shouldBeEmpty.Add);

                DefaultSettings.Cache.CreateCacheFactoryPreset(25, shouldRemain);
                DefaultSettings.Cache.WithLocalCacheFactory(shouldBeRemoved);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(25)
                    .Build();

                DefaultSettings.Cache.WithLocalCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await proxy.StringToString(key);
            
            shouldBePopulated.Should().ContainSingle();
            shouldBePopulated[0].Misses.Should().ContainSingle();
            Assert.Equal(key, shouldBePopulated[0].Misses[0]);
            Assert.Empty(shouldBeEmpty);
        }
        
        [Fact]
        public async Task DefaultDistributedCacheRemovedIfNoneDefinedInPreset()
        {
            var shouldBePopulated = new List<CacheGetResult>();
            var shouldBeEmpty = new List<CacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                var shouldRemain = new TestLocalCacheFactory().OnGetResult(shouldBePopulated.Add);
                var shouldBeRemoved = new TestCacheFactory().OnGetResult(shouldBeEmpty.Add);

                DefaultSettings.Cache.CreateCacheFactoryPreset(26, shouldRemain);
                DefaultSettings.Cache.WithDistributedCacheFactory(shouldBeRemoved);

                proxy = impl
                    .Cached()
                    .WithCacheFactoryPreset(26)
                    .Build();

                DefaultSettings.Cache.WithDistributedCacheFactory(null);
            }

            var key = Guid.NewGuid().ToString();
            
            await proxy.StringToString(key);
            
            shouldBePopulated.Should().ContainSingle();
            shouldBePopulated[0].Misses.Should().ContainSingle();
            Assert.Equal(key, shouldBePopulated[0].Misses[0]);
            Assert.Empty(shouldBeEmpty);
        }
    }
}