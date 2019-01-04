using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class DisableCache : CacheTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisableCacheTests(bool disableCache)
        {
            var fetches = new List<FunctionCacheFetchResult>();

            Func<string, Task<string>> echo = new Echo();
            Func<string, Task<string>> cachedEcho;
            using (EnterSetup(false))
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

                Assert.Equal(disableCache ? i : 1, fetches.Count);
            }
        }
    }
}
