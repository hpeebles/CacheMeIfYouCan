using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Proxy;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    [Collection(TestCollections.Cache)]
    public class CachedProxyTests
    {
        private readonly CacheSetupLock _setupLock;

        public CachedProxyTests(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnResult(bool isEnumerableKey)
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResultObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");
            
            results.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnFetch(bool isEnumerableKey)
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnFetchObservable(x => x.Subscribe(fetches.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            fetches.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnException(bool isEnumerableKey)
        {
            var errors = new List<FunctionCacheException>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                    .OnExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.MultiEcho(new[] { "123" }));
            else
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.StringToString("123"));

            errors.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheGet(bool isEnumerableKey)
        {
            var results = new List<CacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnCacheGetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            results.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheSet(bool isEnumerableKey)
        {
            var results = new List<CacheSetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnCacheSetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            results.Should().ContainSingle();
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheException(bool isEnumerableKey)
        {
            var errors = new List<CacheException>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                    .OnCacheExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            if (isEnumerableKey)
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.MultiEcho(new[] { "123" }));
            else
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.StringToString("123"));
            
            errors.Should().ContainSingle();
        }
    }
}