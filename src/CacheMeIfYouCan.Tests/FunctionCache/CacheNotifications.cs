using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class CacheNotifications
    {
        [Fact]
        public async Task OnCacheGet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .OnCacheGet(results.Add)
                .Build();

            var start = Timestamp.Now;
            
            await cachedEcho("123");
            Assert.Single(results);
            Assert.Equal(0, results[0].HitsCount);
            Assert.Empty(results[0].Hits);
            Assert.Equal(1, results[0].MissesCount);
            Assert.Single(results[0].Misses);
            Assert.Equal("123", results[0].Misses[0]);
            Assert.InRange(results[0].Start, start, Timestamp.Now);
            
            await cachedEcho("123");
            Assert.Equal(2, results.Count);
            Assert.Equal(1, results[1].HitsCount);
            Assert.Single(results[1].Hits);
            Assert.Equal("123", results[1].Hits[0]);
            Assert.Equal(0, results[1].MissesCount);
            Assert.Empty(results[1].Misses);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var results = new List<CacheSetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .OnCacheSet(results.Add)
                .Build();

            var start = Timestamp.Now;
            
            await cachedEcho("123");
            Assert.Single(results);
            Assert.Single(results[0].Keys);
            Assert.Equal("123", results[0].Keys[0]);
            Assert.InRange(results[0].Start, start, Timestamp.Now);
            
            await cachedEcho("123");
            Assert.Single(results);
        }

        [Fact]
        public async Task OnCacheException()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var errors = new List<CacheException>();
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(new TestCache<string, string>(x => x, x => x, error: () => true))
                .OnCacheException(errors.Add)
                .Build();

            var start = Timestamp.Now;
            
            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho("123"));
            
            Assert.Single(errors);
            Assert.Single(errors[0].Keys);
            Assert.Equal("123", errors[0].Keys.First());
            Assert.InRange(errors[0].Timestamp, start, Timestamp.Now);
            Assert.IsType<CacheGetException<string>>(errors[0]);
        }

        [Fact]
        public async Task TwoTierCache()
        {
            Func<string, Task<string>> echo = new Echo();
            
            var getResults = new List<CacheGetResult>();
            var setResults = new List<CacheSetResult>();
            
            var cachedEcho = echo
                .Cached()
                .WithTimeToLive(TimeSpan.FromSeconds(1))
                .WithLocalCache(new TestLocalCache<string, string>())
                .WithDistributedCacheFactory(new TestCacheFactory())
                .OnCacheGet(getResults.Add)
                .OnCacheSet(setResults.Add)
                .Build();

            await cachedEcho("123");
            Assert.Equal(2, getResults.Count);
            Assert.Equal("test-local", getResults[0].CacheType);
            Assert.Equal("test", getResults[1].CacheType);
            Assert.Equal(2, setResults.Count);
            Assert.Equal("test-local", setResults[0].CacheType);
            Assert.Equal("test", setResults[1].CacheType);
            
            await cachedEcho("123");
            Assert.Equal(3, getResults.Count);
            Assert.Equal("test-local", getResults[2].CacheType);
            Assert.Equal(2, setResults.Count);
        }

        [Fact]
        public async Task MultiKey()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            var getResults = new List<CacheGetResult>();
            var setResults = new List<CacheSetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnCacheGet(getResults.Add)
                .OnCacheSet(setResults.Add)
                .Build();

            await cachedEcho(new[] {"123", "abc"});
            Assert.Single(getResults);
            Assert.Equal(0, getResults[0].HitsCount);
            Assert.Empty(getResults[0].Hits);
            Assert.Equal(2, getResults[0].MissesCount);
            Assert.Equal(new[] {"123", "abc"}, getResults[0].Misses);
            Assert.Single(setResults);
            Assert.Equal(new[] {"123", "abc"}, setResults[0].Keys);

            await cachedEcho(new[] {"123", "abc", "456"});
            Assert.Equal(2, getResults.Count);
            Assert.Equal(2, getResults[1].HitsCount);
            Assert.Equal(new[] {"123", "abc"}, getResults[1].Hits);
            Assert.Equal(1, getResults[1].MissesCount);
            Assert.Equal(new[] {"456"}, getResults[1].Misses);
            Assert.Equal(2, setResults.Count);
            Assert.Equal(new[] {"456"}, setResults[1].Keys);
        }

        [Theory]
        [InlineData(AdditionBehaviour.Append)]
        [InlineData(AdditionBehaviour.Prepend)]
        [InlineData(AdditionBehaviour.Overwrite)]
        public async Task CombinedActions(AdditionBehaviour secondActionAdditionBehaviour)
        {
            var list = new List<int>();
            
            Func<string, Task<string>> echo = new Echo();
            
            var cachedEcho = echo
                .Cached()
                .OnResult(x => list.Add(1))
                .OnResult(x => list.Add(2), secondActionAdditionBehaviour)
                .Build();

            await cachedEcho("123");

            if (secondActionAdditionBehaviour == AdditionBehaviour.Append)
            {
                Assert.Equal(2, list.Count);
                Assert.Equal(1, list[0]);
                Assert.Equal(2, list[1]);
            }
            else if (secondActionAdditionBehaviour == AdditionBehaviour.Prepend)
            {
                Assert.Equal(2, list.Count);
                Assert.Equal(2, list[0]);
                Assert.Equal(1, list[1]);
            }
            else if (secondActionAdditionBehaviour == AdditionBehaviour.Overwrite)
            {
                Assert.Single(list);
                Assert.Equal(2, list.Single());
            }
        }
    }
}