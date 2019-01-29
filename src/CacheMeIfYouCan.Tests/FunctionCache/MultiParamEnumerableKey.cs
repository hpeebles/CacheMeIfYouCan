using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class MultiParamEnumerableKey
    {
        private readonly CacheSetupLock _setupLock;

        public MultiParamEnumerableKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public async Task TwoParamKeySucceeds()
        {
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();

            var result = await cachedFunc(outerKey, new[] { 1, 2 });

            result.Should().ContainKeys(1, 2);
        }
        
        [Fact]
        public async Task ThreeParamKeySucceeds()
        {
            Func<string, string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2, k3) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k3.ToDictionary(k => k, k => k1 + k2 + k));
            };
            
            Func<string, string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .Build();
            }

            var outerKey1 = Guid.NewGuid().ToString();
            var outerKey2 = Guid.NewGuid().ToString();

            var result = await cachedFunc(outerKey1, outerKey2, new[] { 1, 2 });

            result.Should().ContainKeys(1, 2);
        }
        
        [Fact]
        public async Task FourParamKeySucceeds()
        {
            Func<string, string, string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2, k3, k4) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k4.ToDictionary(k => k, k => k1 + k2 + k3 + k));
            };
            
            Func<string, string, string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, string, string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .Build();
            }

            var outerKey1 = Guid.NewGuid().ToString();
            var outerKey2 = Guid.NewGuid().ToString();
            var outerKey3 = Guid.NewGuid().ToString();

            var result = await cachedFunc(outerKey1, outerKey2, outerKey3, new[] { 1, 2 });

            result.Should().ContainKeys(1, 2);
        }
        
        [Fact]
        public async Task KeysAlreadyInCacheAreNotFetched()
        {
            var results = new List<FunctionCacheGetResult>();
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> func = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .OnResult(results.Add)
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();

            var result = await cachedFunc(outerKey, new[] { 1, 2 });

            result.Should().ContainKeys(1, 2);
            results.Should().ContainSingle();
            
            foreach (var r in results.Single().Results)
                r.Outcome.Should().Be(Outcome.Fetch);

            result = await cachedFunc(outerKey, new[] { 2, 3 });

            result.Should().ContainKeys(2, 3);
            results.Count.Should().Be(2);

            var ordered = results[1].Results.OrderBy(r => r.KeyString).ToArray();

            ordered[0].Outcome.Should().Be(Outcome.FromCache);
            ordered[1].Outcome.Should().Be(Outcome.Fetch);
        }
        
        [Fact]
        public async Task WithBatchedFetches()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> echo = (k1, k2) =>
            {
                return Task.FromResult<IDictionary<int, string>>(k2.ToDictionary(k => k, k => k1 + k));
            };
            
            Func<string, IEnumerable<int>, Task<IDictionary<int, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<string, IEnumerable<int>, IDictionary<int, string>, int, string>()
                    .OnFetch(fetches.Add)
                    .WithBatchedFetches(2)
                    .Build();
            }

            var outerKey = Guid.NewGuid().ToString();
            var innerKeys = Enumerable.Range(0, 9).ToArray();
            
            var result = await cachedEcho(outerKey, innerKeys);

            result.Should().ContainKeys(innerKeys);
            
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