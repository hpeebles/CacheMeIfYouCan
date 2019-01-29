using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.Proxy
{
    [Collection(TestCollections.Proxy)]
    public class MultiParamEnumerableKey
    {
        private readonly CacheSetupLock _setupLock;

        public MultiParamEnumerableKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task MultiParamEnumerableKeyCacheSucceeds()
        {
            var results = new List<FunctionCacheGetResult>();
            
            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnResult(results.Add)
                    .Build();
            }

            var result1 = await proxy.MultiParamEnumerableKey("abc", new[] { 1, 2 });
            
            result1.Should().ContainKeys(1, 2);
            results.Should().ContainSingle().Which.Results.Should().OnlyContain(r => r.Outcome == Outcome.Fetch);
            
            var result2 = await proxy.MultiParamEnumerableKey("abc", new[] { 2, 3 });

            result2.Should().ContainKeys(2, 3);
            results.Should().HaveCount(2)
                .And.Subject.Last().Results.Should().HaveCount(2)
                .And.Contain(r => r.Outcome == Outcome.FromCache)
                .And.Contain(r => r.Outcome == Outcome.Fetch);
            
            var result3 = await proxy.MultiParamEnumerableKey("xyz", new[] { 1, 2, 3 });

            result3.Should().ContainKeys(1, 2, 3);
            results.Should().HaveCount(3)
                .And.Subject.Last().Results.Should().HaveCount(3)
                .And.OnlyContain(r => r.Outcome == Outcome.Fetch);
        }
    }
}