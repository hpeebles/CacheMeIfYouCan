using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Proxy;
using Xunit;

namespace CacheMeIfYouCan.Tests.ConfigurationExtensions
{
    public class CachedProxyTests : CacheTestBase
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnResult(bool multiKey)
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnResultObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

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
            var fetches = new List<FunctionCacheFetchResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnFetchObservable(x => x.Subscribe(fetches.Add))
                    .Build();
            }

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            Assert.Single(fetches);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnException(bool multiKey)
        {
            var errors = new List<FunctionCacheException>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                    .OnExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

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
            var results = new List<CacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnCacheGetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

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
            var results = new List<CacheSetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .OnCacheSetObservable(x => x.Subscribe(results.Add))
                    .Build();
            }

            if (multiKey)
                await proxy.MultiEcho(new[] { "123" });
            else
                await proxy.StringToString("123");

            Assert.Single(results);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task OnCacheException(bool multiKey)
        {
            var errors = new List<CacheException>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (EnterSetup(false))
            {
                proxy = impl
                    .Cached()
                    .WithDistributedCacheFactory(new TestCacheFactory(error: () => true))
                    .OnCacheExceptionObservable(x => x.Subscribe(errors.Add))
                    .Build();
            }

            if (multiKey)
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.MultiEcho(new[] { "123" }));
            else
                await Assert.ThrowsAnyAsync<FunctionCacheException>(() => proxy.StringToString("123"));
            
            Assert.Single(errors);
        }
    }
}