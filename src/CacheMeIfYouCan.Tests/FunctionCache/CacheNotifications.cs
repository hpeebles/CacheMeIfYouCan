using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class CacheNotifications
    {
        private readonly CacheSetupLock _setupLock;

        public CacheNotifications(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task OnCacheGet()
        {
            var results = new List<CacheGetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .OnCacheGet(results.Add)
                    .Build();
            }

            var start = Timestamp.Now;
            
            await cachedEcho("123");
            results.Should().ContainSingle();
            results[0].HitsCount.Should().Be(0);
            results[0].Hits.Should().BeEmpty();
            results[0].MissesCount.Should().Be(1);
            results[0].Misses.Should().ContainSingle();
            results[0].Misses[0].Should().Be("123");
            results[0].Start.Should().BeInRange(start, Timestamp.Now);
            
            await cachedEcho("123");
            results.Count.Should().Be(2);
            results[1].HitsCount.Should().Be(1);
            results[1].Hits.Should().ContainSingle();
            results[1].Hits[0].Should().Be("123");
            results[1].MissesCount.Should().Be(0);
            results[1].Misses.Should().BeEmpty();
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            var results = new List<CacheSetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .OnCacheSet(results.Add)
                    .Build();
            }

            var start = Timestamp.Now;
            
            await cachedEcho("123");
            results.Should().ContainSingle();
            results[0].Keys.Should().ContainSingle();
            results[0].Keys[0].Should().Be("123");
            results[0].Start.Should().BeInRange(start, Timestamp.Now);
            
            await cachedEcho("123");
            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnCacheRemove()
        {
            var results = new List<CacheRemoveResult>();

            var keysToRemove = new Subject<string>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .OnCacheRemove(results.Add)
                    .WithKeysToRemoveObservable(keysToRemove)
                    .Build();
            }

            var start = Timestamp.Now;
            
            await cachedEcho("123");
            
            keysToRemove.OnNext("123");
            
            results.Should().ContainSingle();
            results[0].Key.Should().Be("123");
            results[0].KeyRemoved.Should().BeTrue();
            results[0].Start.Should().BeInRange(start, Timestamp.Now);
            
            keysToRemove.OnNext("123");

            results.Should().HaveCount(2);
            results[1].KeyRemoved.Should().BeFalse();
        }

        [Fact]
        public async Task OnCacheException()
        {
            var errors = new List<CacheException>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithDistributedCache(new TestCache<string, string>(x => x, x => x, error: () => true))
                    .OnCacheException(errors.Add)
                    .Build();
            }

            var start = Timestamp.Now;

            Func<Task<string>> func = () => cachedEcho("123");
            await func.Should().ThrowAsync<FunctionCacheException>();
            
            errors.Should().ContainSingle();
            errors[0].Keys.Should().ContainSingle();
            errors[0].Keys.First().Should().Be("123");
            errors[0].Timestamp.Should().BeInRange(start, Timestamp.Now);
            errors[0].Should().BeOfType<CacheGetException<string>>();
        }

        [Fact]
        public async Task TwoTierCache()
        {
            var getResults = new List<CacheGetResult>();
            var setResults = new List<CacheSetResult>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .WithTimeToLive(TimeSpan.FromSeconds(1))
                    .WithLocalCache(new TestLocalCache<string, string>())
                    .WithDistributedCacheFactory(new TestCacheFactory())
                    .OnCacheGet(getResults.Add)
                    .OnCacheSet(setResults.Add)
                    .Build();
            }

            await cachedEcho("123");
            getResults.Count.Should().Be(2);
            getResults[0].CacheType.Should().Be("test-local");
            getResults[1].CacheType.Should().Be("test");
            setResults.Count.Should().Be(2);
            setResults[0].CacheType.Should().Be("test-local");
            setResults[1].CacheType.Should().Be("test");
            
            await cachedEcho("123");
            getResults.Count.Should().Be(3);
            getResults[2].CacheType.Should().Be("test-local");
            setResults.Count.Should().Be(2);
        }

        [Fact]
        public async Task EnumerableKey()
        {
            var getResults = new List<CacheGetResult>();
            var setResults = new List<CacheSetResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnCacheGet(getResults.Add)
                    .OnCacheSet(setResults.Add)
                    .Build();
            }

            await cachedEcho(new[] {"123", "abc"});
            getResults.Should().ContainSingle();
            getResults[0].HitsCount.Should().Be(0);
            getResults[0].Hits.Should().BeEmpty();
            getResults[0].MissesCount.Should().Be(2);
            getResults[0].Misses.Should().BeEquivalentTo("123", "abc");
            setResults.Should().ContainSingle();
            setResults[0].Keys.Should().BeEquivalentTo("123", "abc");

            await cachedEcho(new[] {"123", "abc", "456"});
            getResults.Count.Should().Be(2);
            getResults[1].HitsCount.Should().Be(2);
            getResults[1].Hits.Should().BeEquivalentTo("123", "abc");
            getResults[1].MissesCount.Should().Be(1);
            getResults[1].Misses.Should().BeEquivalentTo("456");
            setResults.Count.Should().Be(2);
            setResults[1].Keys.Should().BeEquivalentTo("456");
        }

        [Theory]
        [InlineData(AdditionBehaviour.Append)]
        [InlineData(AdditionBehaviour.Prepend)]
        [InlineData(AdditionBehaviour.Overwrite)]
        public async Task CombinedActions(AdditionBehaviour secondActionAdditionBehaviour)
        {
            var list = new List<int>();
            
            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .OnResult(x => list.Add(1))
                    .OnResult(x => list.Add(2), secondActionAdditionBehaviour)
                    .Build();
            }

            await cachedEcho("123");

            if (secondActionAdditionBehaviour == AdditionBehaviour.Append)
            {
                list.Count.Should().Be(2);
                list[0].Should().Be(1);
                list[1].Should().Be(2);
            }
            else if (secondActionAdditionBehaviour == AdditionBehaviour.Prepend)
            {
                list.Count.Should().Be(2);
                list[0].Should().Be(2);
                list[1].Should().Be(1);
            }
            else if (secondActionAdditionBehaviour == AdditionBehaviour.Overwrite)
            {
                list.Should().ContainSingle();
                list.Single().Should().Be(2);
            }
        }
    }
}