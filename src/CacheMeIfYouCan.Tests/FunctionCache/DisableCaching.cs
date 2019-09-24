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
    public class DisableCaching
    {
        private readonly CacheSetupLock _setupLock;

        public DisableCaching(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisableCachingTests(bool disableCaching)
        {
            var fetches = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached()
                    .DisableCaching(disableCaching)
                    .OnFetch(fetches.Add)
                    .Build();
            }

            for (var i = 1; i < 10; i++)
            {
                await cachedEcho("abc");

                fetches.Count.Should().Be(disableCaching ? i : 1);
            }
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public async Task DisableCachingCascades(bool disableDefault, bool disableProxy, bool disableFunction)
        {
            var fetches = new List<FunctionCacheFetchResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter(true))
            {
                DefaultSettings.Cache.DisableCaching(disableDefault);
                
                proxy = impl
                    .Cached()
                    .DisableCaching(disableProxy)
                    .OnFetch(fetches.Add)
                    .ConfigureFor<string, string>(x => x.StringToString, c => c.DisableCaching(disableFunction))
                    .Build();
                
                DefaultSettings.Cache.DisableCaching(false);
            }

            var disableCaching = disableDefault || disableProxy || disableFunction;
            
            for (var i = 1; i < 10; i++)
            {
                await proxy.StringToString("abc");

                fetches.Count.Should().Be(disableCaching ? i : 1);
            }
        }
    }
}
