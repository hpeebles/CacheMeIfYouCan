using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class MultiParamKey
    {
        private readonly CacheSetupLock _setupLock;

        public MultiParamKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task TwoParamKeySucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, int, Task<string>> func = (k1, k2) => Task.FromResult($"{k1}_{k2}");
            Func<string, int, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key1 = Guid.NewGuid().ToString();
            var key2 = 2;

            var result = await cachedFunc(key1, key2);
            
            Assert.Equal($"{key1}_{key2}", result);
            results.Should().ContainSingle();
            results.Single().Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            result = await cachedFunc(key1, key2);
            
            Assert.Equal($"{key1}_{key2}", result);
            Assert.Equal(2, results.Count);
            results.Last().Results.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
        }
        
        [Fact]
        public async Task ThreeParamKeySucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, int, int, Task<string>> func = (k1, k2, k3) => Task.FromResult($"{k1}_{k2}_{k3}");
            Func<string, int, int, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key1 = Guid.NewGuid().ToString();
            var key2 = 2;
            var key3 = 3;

            var result = await cachedFunc(key1, key2, key3);
            
            Assert.Equal($"{key1}_{key2}_{key3}", result);
            results.Should().ContainSingle();
            results.Single().Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            result = await cachedFunc(key1, key2, key3);
            
            Assert.Equal($"{key1}_{key2}_{key3}", result);
            Assert.Equal(2, results.Count);
            results.Last().Results.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
        }
        
        [Fact]
        public async Task FourParamKeySucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, int, int, int, Task<string>> func = (k1, k2, k3, k4) => Task.FromResult($"{k1}_{k2}_{k3}_{k4}");
            Func<string, int, int, int, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var key1 = Guid.NewGuid().ToString();
            var key2 = 2;
            var key3 = 3;
            var key4 = 4;
            
            var result = await cachedFunc(key1, key2, key3, key4);
            
            Assert.Equal($"{key1}_{key2}_{key3}_{key4}", result);
            results.Should().ContainSingle();
            results.Single().Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results.Single().Results.Single().Outcome);

            result = await cachedFunc(key1, key2, key3, key4);
            
            Assert.Equal($"{key1}_{key2}_{key3}_{key4}", result);
            Assert.Equal(2, results.Count);
            results.Last().Results.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results.Last().Results.Single().Outcome);
        }

        [Fact]
        public async Task ExcludeParametersFromKey()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, int, Task<string>> func = (k1, k2) => Task.FromResult($"{k1}_{k2}");
            Func<string, int, Task<string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached()
                    .OnResult(results.Add)
                    .ExcludeParametersFromKey(0)
                    .Build();
            }

            var key1 = Guid.NewGuid().ToString();
            var key2 = 2;

            await cachedFunc(key1, key2);
            
            var result = await cachedFunc(Guid.NewGuid().ToString(), key2);
            
            result.Should().Be($"{key1}_{key2}");
            results.Last().Results.Single().Outcome.Should().Be(Outcome.FromCache);
        }
        
        [Fact]
        public void ExcludeParametersFromKeyFailsIfAllKeysAreExcluded()
        {
            Func<string, int, Task<string>> func = (k1, k2) => Task.FromResult($"{k1}_{k2}");
            using (_setupLock.Enter())
            {
                Action action = () => func
                    .Cached()
                    .ExcludeParametersFromKey(0, 1)
                    .Build();

                action.Should().Throw<ArgumentOutOfRangeException>();
            }
        }
    }
}