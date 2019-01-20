using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    [Collection(TestCollections.Cache)]
    public class MultiKeyFunctionCacheTests
    {
        private readonly CacheSetupLock _setupLock;

        public MultiKeyFunctionCacheTests(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task OnResult()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnResultObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho(new[] { "123" });

            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnFetch()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnFetchObservable(x => x.Subscribe(fetches.Add))
                    .Build();
            }

            await cachedEcho(new[] { "123" });

            fetches.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnException()
        {
            var errors = new List<FunctionCacheException>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.Zero, () => true);
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(new[] { "123" }));

            Assert.Equal(2, errors.Count);
        }
        
        
        [Fact]
        public async Task OnCacheGet()
        {
            var results = new List<CacheGetResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnCacheGetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho(new[] { "123" });

            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnCacheSet()
        {
            var results = new List<CacheSetResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnCacheSetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            await cachedEcho(new[] { "123" });

            results.Should().ContainSingle();
        }
        
        [Fact]
        public async Task OnCacheException()
        {
            var errors = new List<CacheException>();

            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.Zero, () => true);
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                var cache = new TestCacheFactory(error: () => true)
                    .Build<string, string>("test");

                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithDistributedCache(cache)
                    .OnCacheExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            await Assert.ThrowsAnyAsync<FunctionCacheException>(() => cachedEcho(new[] { "123" }));

            errors.Should().ContainSingle();
        }
    }
}