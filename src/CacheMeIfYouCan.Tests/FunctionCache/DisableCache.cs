using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class DisableCache
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisableCacheTests(bool disableCache)
        {
            Func<string, Task<string>> echo = new Echo();

            var fetches = new List<FunctionCacheFetchResult>();

            var cachedEcho = echo
                .Cached()
                .DisableCache(disableCache)
                .OnFetch(fetches.Add)
                .Build();

            for (var i = 1; i < 10; i++)
            {
                await cachedEcho("abc");

                Assert.Equal(disableCache ? i : 1, fetches.Count);
            }
        }
    }
}
