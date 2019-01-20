using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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
    }
}