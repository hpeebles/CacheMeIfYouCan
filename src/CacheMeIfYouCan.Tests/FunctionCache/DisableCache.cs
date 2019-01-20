using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
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
    }
}
