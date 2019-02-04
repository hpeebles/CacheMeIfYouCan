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
    public class EnumerableKey
    {
        private readonly CacheSetupLock _setupLock;

        public EnumerableKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }
        
        [Fact]
        public async Task EnumerableKeyCacheIsProduced()
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

            await proxy.MultiEcho(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ListParameterSucceeds()
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

            await proxy.MultiEchoList(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task SetParameterSucceeds()
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

            await proxy.MultiEchoSet(new HashSet<string> { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
        }

        [Fact]
        public async Task ConcurrentDictionaryReturnValueSucceeds()
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

            await proxy.MultiEchoToConcurrent(new[] { "123", "abc" });
            
            results.Should().ContainSingle();
            Assert.Equal(2, results.Single().Results.Count);
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

            var keys = Enumerable.Range(0, 9).Select(i => i.ToString()).ToArray();
            
            var results = await proxy.MultiEcho(keys);

            results.Should().ContainKeys(keys);
            
            fetches.Should().HaveCount(5);

            var ordered = fetches.OrderBy(f => f.Results.First().KeyString).ToArray();

            ordered[0].Results.Select(r => r.KeyString).Should().BeEquivalentTo("0", "1");
            ordered[1].Results.Select(r => r.KeyString).Should().BeEquivalentTo("2", "3");
            ordered[2].Results.Select(r => r.KeyString).Should().BeEquivalentTo("4", "5");
            ordered[3].Results.Select(r => r.KeyString).Should().BeEquivalentTo("6", "7");
            ordered[4].Results.Select(r => r.KeyString).Should().BeEquivalentTo("8");
        }
        
        [Fact]
        public async Task WithBatchedFetchesMultiParam()
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

            ordered[0].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "0", outerKey + "1");
            ordered[1].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "2", outerKey + "3");
            ordered[2].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "4", outerKey + "5");
            ordered[3].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "6", outerKey + "7");
            ordered[4].Results.Select(r => r.KeyString).Should().BeEquivalentTo(outerKey + "8");
        }
    }
}