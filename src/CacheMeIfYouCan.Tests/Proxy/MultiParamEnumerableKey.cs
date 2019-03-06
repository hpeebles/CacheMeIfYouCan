using System;
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

        [Fact]
        public async Task WithBatchedFetches()
        {
            var fetches = new List<FunctionCacheFetchResult>();

            ITest impl = new TestImpl();
            ITest proxy;
            using (_setupLock.Enter())
            {
                proxy = impl
                    .Cached()
                    .OnFetch(fetches.Add)
                    .WithBatchedFetches(2)
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();
            var innerKeys = Enumerable.Range(0, 9).ToArray();

            var results = await proxy.MultiParamEnumerableKey(outerKey, innerKeys);

            results.Should().ContainKeys(innerKeys);

            fetches.Should().HaveCount(5);

            var ordered = fetches.OrderBy(f => f.Results.First().KeyString).ToArray();

            ordered[0].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "_0", outerKey + "_1");
            ordered[1].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "_2", outerKey + "_3");
            ordered[2].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "_4", outerKey + "_5");
            ordered[3].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "_6", outerKey + "_7");
            ordered[4].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "_8");
        }
    }
}