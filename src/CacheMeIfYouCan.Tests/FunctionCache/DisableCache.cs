using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
using CacheMeIfYouCan.Tests.Proxy;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class DisableCache
    {
        private readonly CacheSetupLock _setupLock;

        public DisableCache(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisableCacheTests(bool disableCache)
        {
            var fetches = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .DisableCache(disableCache)
                    .OnFetch(fetches.Add)
                    .Build();
            }

            for (var i = 1; i < 10; i++)
            {
                await cachedEcho("abc");

                fetches.Count.Should().Be(disableCache ? i : 1);
            }
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task DisableCacheCascades(bool disableDefault, bool disableProxy, bool disableFunction)
        {
            var fetches = new List<FunctionCacheFetchResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.DisableCache(disableDefault);
                
                proxy = impl
                    .Cached()
                    .DisableCache(disableProxy)
                    .OnFetch(fetches.Add)
                    .ConfigureFor<string, string>(x => x.StringToString, c => c.DisableCache(disableFunction))
                    .Build();
                
                DefaultSettings.Cache.DisableCache(false);
            }

            var disableCache = disableDefault || disableProxy || disableFunction;
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.StringToString("abc");

                fetches.Count.Should().Be(disableCache ? i : 1);
            }
        }
    }
}
