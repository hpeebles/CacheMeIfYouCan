using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class MultiKeyFunctionCacheTests
    {
        [Fact]
        public async Task OnResult()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            var results = new List<FunctionCacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnResultObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho(new[] { "123" });

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnFetch()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            var fetches = new List<FunctionCacheFetchResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnFetchObservable(x => x.Subscribe(fetches.Add))
                .Build();

            await cachedEcho(new[] { "123" });

            Assert.Single(fetches);
        }
        
        [Fact]
        public async Task OnError()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.Zero, () => true);
            
            var errors = new List<FunctionCacheException>();
            
            var cachedEcho = echo
                .Cached()
                .OnErrorObservable(x => x.Subscribe(errors.Add))
                .Build();

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(new[] { "123" }));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            var results = new List<CacheGetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnCacheGetObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho(new[] { "123" });

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            
            var results = new List<CacheSetResult>();
            
            var cachedEcho = echo
                .Cached()
                .OnCacheSetObservable(x => x.Subscribe(results.Add))
                .Build();

            await cachedEcho(new[] { "123" });

            Assert.Single(results);
        }
        
        [Fact]
        public async Task OnCacheError()
        {
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.Zero, () => true);
            
            var errors = new List<CacheException>();

            var cache = new TestCacheFactory(error: () => true)
                .Configure(x => { })
                .Build(new DistributedCacheFactoryConfig<string, string>());
            
            var cachedEcho = echo
                .Cached()
                .WithDistributedCache(cache)
                .OnCacheErrorObservable(x => x.Subscribe(errors.Add))
                .Build();

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(new[] { "123" }));

            Assert.Single(errors);
        }
    }
}