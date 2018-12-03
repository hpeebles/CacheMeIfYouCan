using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Proxy;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class CachedProxyTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnResult(bool multiKey)
        {
            ITest impl = new TestImpl();

            var results = new List<FunctionCacheGetResult>();
            
            var proxy = impl
                .Cached()
                .OnResultObservable(x => x.Subscribe(results.Add))
                .Build();

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");
            
            Assert.Single(results);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnFetch(bool multiKey)
        {
            ITest impl = new TestImpl();

            var fetches = new List<FunctionCacheFetchResult>();
            
            var proxy = impl
                .Cached()
                .OnFetchObservable(x => x.Subscribe(fetches.Add))
                .Build();

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            Assert.Single(fetches);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnError(bool multiKey)
        {
            ITest impl = new TestImpl();

            var errors = new List<FunctionCacheException>();
            
            var proxy = impl
                .Cached()
                .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                .OnErrorObservable(x => x.Subscribe(errors.Add))
                .Build();
            
            if (multiKey)
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.MultiEcho(new[] { "123" }));
            else
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.StringToString("123"));

            Assert.Single(errors);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheGet(bool multiKey)
        {
            ITest impl = new TestImpl();

            var results = new List<CacheGetResult>();
            
            var proxy = impl
                .Cached()
                .OnCacheGetObservable(x => x.Subscribe(results.Add))
                .Build();

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            Assert.Single(results);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheSet(bool multiKey)
        {
            ITest impl = new TestImpl();

            var results = new List<CacheSetResult>();
            
            var proxy = impl
                .Cached()
                .OnCacheSetObservable(x => x.Subscribe(results.Add))
                .Build();

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            Assert.Single(results);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheError(bool multiKey)
        {
            ITest impl = new TestImpl();

            var errors = new List<CacheException>();
            
            var proxy = impl
                .Cached()
                .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                .OnCacheErrorObservable(x => x.Subscribe(errors.Add))
                .Build();
            
            if (multiKey)
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.MultiEcho(new[] { "123" }));
            else
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.StringToString("123"));
            
            Assert.Single(errors);
        }
    }
}