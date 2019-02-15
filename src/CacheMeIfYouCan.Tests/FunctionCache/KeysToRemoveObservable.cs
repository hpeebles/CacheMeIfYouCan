using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class KeysToRemoveObservable
    {
        private readonly CacheSetupLock _setupLock;

        public KeysToRemoveObservable(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task RemovesKeysFromCache()
        {
            var results = new List<FunctionCacheGetResult>();

            var keysToRemoveObservable = new Subject<string>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnResult(results.Add)
                    .WithKeysToRemoveObservable(keysToRemoveObservable)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await cachedEcho(key);

            results.Should().ContainSingle();
            results[0].Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results[0].Results.First().Outcome);

            await cachedEcho(key);
            
            Assert.Equal(2, results.Count);
            results[1].Results.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results[1].Results.First().Outcome);
            
            keysToRemoveObservable.OnNext(key);
            
            await cachedEcho(key);

            Assert.Equal(3, results.Count);
            results[2].Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results[2].Results.First().Outcome);
        }

        [Fact]
        public async Task ContinuesToWorkAfterException()
        {
            var results = new List<FunctionCacheGetResult>();

            var keysToRemoveObservable = new Subject<string>();

            var error = false;
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(new TestLocalCache<string, string>(error: () => error))
                    .OnResult(results.Add)
                    .WithKeysToRemoveObservable(keysToRemoveObservable)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await cachedEcho(key);

            results.Should().ContainSingle();
            results[0].Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results[0].Results.First().Outcome);

            await cachedEcho(key);
            
            Assert.Equal(2, results.Count);
            results[1].Results.Should().ContainSingle();
            Assert.Equal(Outcome.FromCache, results[1].Results.First().Outcome);

            error = true;
            
            keysToRemoveObservable.OnNext("this will throw");
            
            error = false;
            
            keysToRemoveObservable.OnNext(key);
            
            await cachedEcho(key);

            Assert.Equal(3, results.Count);
            results[2].Results.Should().ContainSingle();
            Assert.Equal(Outcome.Fetch, results[2].Results.First().Outcome);
        }

        [Fact]
        public async Task MultipleObservablesSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();

            var observables = Enumerable
                .Range(0, 5)
                .Select(i => new Subject<string>())
                .ToArray();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                var configManager = echo
                    .Cached()
                    .OnResult(results.Add);
                    
                foreach (var observable in observables)
                    configManager.WithKeysToRemoveObservable(observable);
                
                cachedEcho = configManager.Build();
            }

            foreach (var observable in observables)
            {
                var key = Guid.NewGuid().ToString();

                await cachedEcho(key);

                results.Last().Results.Should().ContainSingle();
                Assert.Equal(Outcome.Fetch, results.Last().Results.First().Outcome);

                await cachedEcho(key);

                results.Last().Results.Should().ContainSingle();
                Assert.Equal(Outcome.FromCache, results.Last().Results.First().Outcome);

                observable.OnNext(key);

                await cachedEcho(key);

                results.Last().Results.Should().ContainSingle();
                Assert.Equal(Outcome.Fetch, results.Last().Results.First().Outcome);
            }
        }

        [Fact]
        public async Task CanRemoveFromLocalOnly()
        {
            var results = new List<FunctionCacheGetResult>();

            var keysToRemoveObservable = new Subject<string>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithLocalCache(new TestLocalCache<string, string>())
                    .WithDistributedCache(new TestCache<string, string>(k => k, k => k))
                    .OnResult(results.Add)
                    .WithKeysToRemoveObservable(keysToRemoveObservable, true)
                    .Build();
            }

            var key = Guid.NewGuid().ToString();

            await cachedEcho(key);

            results.Should().ContainSingle();
            results[0].Results.Should().ContainSingle();
            results[0].Results.First().Outcome.Should().Be(Outcome.Fetch);

            await cachedEcho(key);

            results.Should().HaveCount(2);
            results[1].Results.Should().ContainSingle();
            results[1].Results.First().CacheType.Should().Be("test-local");
            
            keysToRemoveObservable.OnNext(key);

            await cachedEcho(key);
            
            results.Should().HaveCount(3);
            results[2].Results.Should().ContainSingle();
            results[2].Results.First().CacheType.Should().Be("test");
        }
    }
}