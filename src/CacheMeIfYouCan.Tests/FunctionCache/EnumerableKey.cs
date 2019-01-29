using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using FluentAssertions;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    [Collection(TestCollections.FunctionCache)]
    public class EnumerableKey
    {
        private readonly CacheSetupLock _setupLock;

        public EnumerableKey(CacheSetupLock setupLock)
        {
            _setupLock = setupLock;
        }

        [Fact]
        public void EnumerableKeyCacheGetsBuilt()
        {
            // Supported parameter types
            // var parameterTypes = new[]
            // {
            //     typeof(IEnumerable<string>),
            //     typeof(IList<string>),
            //     typeof(List<string>),
            //     typeof(string[]),
            //     typeof(ICollection<string>),
            //     typeof(ISet<string>),
            //     typeof(HashSet<string>),
            //     typeof(SortedSet<string>)
            // };
            // 
            // // Supported return types (+ async versions)
            // var returnTypes = new[]
            // {
            //     typeof(IDictionary<string, string>),
            //     typeof(Dictionary<string, string>),
            //     typeof(SortedDictionary<string, string>),
            //     typeof(ConcurrentDictionary<string, string>)
            // };
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> a1 = null;
            Func<IEnumerable<string>, Task<Dictionary<string, string>>> a2 = null;
            Func<IEnumerable<string>, Task<ConcurrentDictionary<string, string>>> a3 = null;
            Func<IEnumerable<string>, Task<SortedDictionary<string, string>>> a4 = null;
            
            Func<IEnumerable<string>, IDictionary<string, string>> b1 = null;
            Func<IEnumerable<string>, Dictionary<string, string>> b2 = null;
            Func<IEnumerable<string>, ConcurrentDictionary<string, string>> b3 = null;
            Func<IEnumerable<string>, SortedDictionary<string, string>> b4 = null;
            
            Func<IList<string>, Task<IDictionary<string, string>>> c1 = null;
            Func<IList<string>, Task<Dictionary<string, string>>> c2 = null;
            Func<IList<string>, Task<ConcurrentDictionary<string, string>>> c3 = null;
            Func<IList<string>, Task<SortedDictionary<string, string>>> c4 = null;
            
            Func<IList<string>, IDictionary<string, string>> d1 = null;
            Func<IList<string>, Dictionary<string, string>> d2 = null;
            Func<IList<string>, ConcurrentDictionary<string, string>> d3 = null;
            Func<IList<string>, SortedDictionary<string, string>> d4 = null;

            Func<List<string>, Task<IDictionary<string, string>>> e1 = null;
            Func<List<string>, Task<Dictionary<string, string>>> e2 = null;
            Func<List<string>, Task<ConcurrentDictionary<string, string>>> e3 = null;
            Func<List<string>, Task<SortedDictionary<string, string>>> e4 = null;
            
            Func<List<string>, IDictionary<string, string>> f1 = null;
            Func<List<string>, Dictionary<string, string>> f2 = null;
            Func<List<string>, ConcurrentDictionary<string, string>> f3 = null;
            Func<List<string>, SortedDictionary<string, string>> f4 = null;
            
            Func<string[], Task<IDictionary<string, string>>> g1 = null;
            Func<string[], Task<Dictionary<string, string>>> g2 = null;
            Func<string[], Task<ConcurrentDictionary<string, string>>> g3 = null;
            Func<string[], Task<SortedDictionary<string, string>>> g4 = null;
            
            Func<string[], IDictionary<string, string>> h1 = null;
            Func<string[], Dictionary<string, string>> h2 = null;
            Func<string[], ConcurrentDictionary<string, string>> h3 = null;
            Func<string[], SortedDictionary<string, string>> h4 = null;
            
            Func<ICollection<string>, Task<IDictionary<string, string>>> i1 = null;
            Func<ICollection<string>, Task<Dictionary<string, string>>> i2 = null;
            Func<ICollection<string>, Task<ConcurrentDictionary<string, string>>> i3 = null;
            Func<ICollection<string>, Task<SortedDictionary<string, string>>> i4 = null;
            
            Func<ICollection<string>, IDictionary<string, string>> j1 = null;
            Func<ICollection<string>, Dictionary<string, string>> j2 = null;
            Func<ICollection<string>, ConcurrentDictionary<string, string>> j3 = null;
            Func<ICollection<string>, SortedDictionary<string, string>> j4 = null;
            
            Func<ISet<string>, Task<IDictionary<string, string>>> k1 = null;
            Func<ISet<string>, Task<Dictionary<string, string>>> k2 = null;
            Func<ISet<string>, Task<ConcurrentDictionary<string, string>>> k3 = null;
            Func<ISet<string>, Task<SortedDictionary<string, string>>> k4 = null;
            
            Func<ISet<string>, IDictionary<string, string>> l1 = null;
            Func<ISet<string>, Dictionary<string, string>> l2 = null;
            Func<ISet<string>, ConcurrentDictionary<string, string>> l3 = null;
            Func<ISet<string>, SortedDictionary<string, string>> l4 = null;

            Func<HashSet<string>, Task<IDictionary<string, string>>> m1 = null;
            Func<HashSet<string>, Task<Dictionary<string, string>>> m2 = null;
            Func<HashSet<string>, Task<ConcurrentDictionary<string, string>>> m3 = null;
            Func<HashSet<string>, Task<SortedDictionary<string, string>>> m4 = null;
            
            Func<HashSet<string>, IDictionary<string, string>> n1 = null;
            Func<HashSet<string>, Dictionary<string, string>> n2 = null;
            Func<HashSet<string>, ConcurrentDictionary<string, string>> n3 = null;
            Func<HashSet<string>, SortedDictionary<string, string>> n4 = null;
            
            Func<SortedSet<string>, Task<IDictionary<string, string>>> o1 = null;
            Func<SortedSet<string>, Task<Dictionary<string, string>>> o2 = null;
            Func<SortedSet<string>, Task<ConcurrentDictionary<string, string>>> o3 = null;
            Func<SortedSet<string>, Task<SortedDictionary<string, string>>> o4 = null;
            
            Func<SortedSet<string>, IDictionary<string, string>> p1 = null;
            Func<SortedSet<string>, Dictionary<string, string>> p2 = null;
            Func<SortedSet<string>, ConcurrentDictionary<string, string>> p3 = null;
            Func<SortedSet<string>, SortedDictionary<string, string>> p4 = null;
            
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, IDictionary<string, string>, string, string>>(a1.Cached<IEnumerable<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, Dictionary<string, string>, string, string>>(a2.Cached<IEnumerable<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>>(a3.Cached<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IEnumerable<string>, SortedDictionary<string, string>, string, string>>(a4.Cached<IEnumerable<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IEnumerable<string>, IDictionary<string, string>, string, string>>(b1.Cached<IEnumerable<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IEnumerable<string>, Dictionary<string, string>, string, string>>(b2.Cached<IEnumerable<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>>(b3.Cached<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IEnumerable<string>, SortedDictionary<string, string>, string, string>>(b4.Cached<IEnumerable<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IList<string>, IDictionary<string, string>, string, string>>(c1.Cached<IList<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IList<string>, Dictionary<string, string>, string, string>>(c2.Cached<IList<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IList<string>, ConcurrentDictionary<string, string>, string, string>>(c3.Cached<IList<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<IList<string>, SortedDictionary<string, string>, string, string>>(c4.Cached<IList<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IList<string>, IDictionary<string, string>, string, string>>(d1.Cached<IList<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IList<string>, Dictionary<string, string>, string, string>>(d2.Cached<IList<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IList<string>, ConcurrentDictionary<string, string>, string, string>>(d3.Cached<IList<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<IList<string>, SortedDictionary<string, string>, string, string>>(d4.Cached<IList<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<List<string>, IDictionary<string, string>, string, string>>(e1.Cached<List<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<List<string>, Dictionary<string, string>, string, string>>(e2.Cached<List<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<List<string>, ConcurrentDictionary<string, string>, string, string>>(e3.Cached<List<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<List<string>, SortedDictionary<string, string>, string, string>>(e4.Cached<List<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<List<string>, IDictionary<string, string>, string, string>>(f1.Cached<List<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<List<string>, Dictionary<string, string>, string, string>>(f2.Cached<List<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<List<string>, ConcurrentDictionary<string, string>, string, string>>(f3.Cached<List<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<List<string>, SortedDictionary<string, string>, string, string>>(f4.Cached<List<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<string[], IDictionary<string, string>, string, string>>(g1.Cached<string[], IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<string[], Dictionary<string, string>, string, string>>(g2.Cached<string[], Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<string[], ConcurrentDictionary<string, string>, string, string>>(g3.Cached<string[], ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<string[], SortedDictionary<string, string>, string, string>>(g4.Cached<string[], SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<string[], IDictionary<string, string>, string, string>>(h1.Cached<string[], IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<string[], Dictionary<string, string>, string, string>>(h2.Cached<string[], Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<string[], ConcurrentDictionary<string, string>, string, string>>(h3.Cached<string[], ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<string[], SortedDictionary<string, string>, string, string>>(h4.Cached<string[], SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ICollection<string>, IDictionary<string, string>, string, string>>(i1.Cached<ICollection<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ICollection<string>, Dictionary<string, string>, string, string>>(i2.Cached<ICollection<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ICollection<string>, ConcurrentDictionary<string, string>, string, string>>(i3.Cached<ICollection<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ICollection<string>, SortedDictionary<string, string>, string, string>>(i4.Cached<ICollection<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ICollection<string>, IDictionary<string, string>, string, string>>(j1.Cached<ICollection<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ICollection<string>, Dictionary<string, string>, string, string>>(j2.Cached<ICollection<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ICollection<string>, ConcurrentDictionary<string, string>, string, string>>(j3.Cached<ICollection<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ICollection<string>, SortedDictionary<string, string>, string, string>>(j4.Cached<ICollection<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ISet<string>, IDictionary<string, string>, string, string>>(k1.Cached<ISet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ISet<string>, Dictionary<string, string>, string, string>>(k2.Cached<ISet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ISet<string>, ConcurrentDictionary<string, string>, string, string>>(k3.Cached<ISet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<ISet<string>, SortedDictionary<string, string>, string, string>>(k4.Cached<ISet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ISet<string>, IDictionary<string, string>, string, string>>(l1.Cached<ISet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ISet<string>, Dictionary<string, string>, string, string>>(l2.Cached<ISet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ISet<string>, ConcurrentDictionary<string, string>, string, string>>(l3.Cached<ISet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<ISet<string>, SortedDictionary<string, string>, string, string>>(l4.Cached<ISet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<HashSet<string>, IDictionary<string, string>, string, string>>(m1.Cached<HashSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<HashSet<string>, Dictionary<string, string>, string, string>>(m2.Cached<HashSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<HashSet<string>, ConcurrentDictionary<string, string>, string, string>>(m3.Cached<HashSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<HashSet<string>, SortedDictionary<string, string>, string, string>>(m4.Cached<HashSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<HashSet<string>, IDictionary<string, string>, string, string>>(n1.Cached<HashSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<HashSet<string>, Dictionary<string, string>, string, string>>(n2.Cached<HashSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<HashSet<string>, ConcurrentDictionary<string, string>, string, string>>(n3.Cached<HashSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<HashSet<string>, SortedDictionary<string, string>, string, string>>(n4.Cached<HashSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<SortedSet<string>, IDictionary<string, string>, string, string>>(o1.Cached<SortedSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<SortedSet<string>, Dictionary<string, string>, string, string>>(o2.Cached<SortedSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>>(o3.Cached<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManager<SortedSet<string>, SortedDictionary<string, string>, string, string>>(o4.Cached<SortedSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<SortedSet<string>, IDictionary<string, string>, string, string>>(p1.Cached<SortedSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<SortedSet<string>, Dictionary<string, string>, string, string>>(p2.Cached<SortedSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>>(p3.Cached<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSync<SortedSet<string>, SortedDictionary<string, string>, string, string>>(p4.Cached<SortedSet<string>, SortedDictionary<string, string>, string, string>());
        }
        
        [Fact]
        public async Task OnlyFetchKeysWhichAreNotAlreadyCached()
        {
            FunctionCacheGetResult mostRecentResult = null;
            FunctionCacheFetchResult mostRecentFetch = null;
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithLocalCache(new TestLocalCache<string, string>())
                    .OnResult(r => mostRecentResult = r)
                    .OnFetch(f => mostRecentFetch = f)
                    .Build();
            }

            for (var i = 1; i < 10; i++)
            {
                var keys = Enumerable
                    .Range(1, i)
                    .Select(x => x.ToString())
                    .ToArray();
                
                await cachedEcho(keys);
                
                Assert.Equal(keys.Length, mostRecentResult.Results.Count);
                Assert.Equal(Outcome.Fetch, mostRecentResult.Results.Last().Outcome);
                Assert.All(mostRecentResult.Results.Take(mostRecentResult.Results.Count - 1), x => Assert.Equal(Outcome.FromCache, x.Outcome));

                mostRecentFetch.Results.Should().ContainSingle();
                Assert.Equal(i.ToString(), mostRecentFetch.Results.Single().KeyString);
            }
        }
        
        [Fact]
        public async Task WithBatchedFetches()
        {
            var fetches = new List<FunctionCacheFetchResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnFetch(fetches.Add)
                    .WithBatchedFetches(2)
                    .Build();
            }

            var keys = Enumerable.Range(0, 9).Select(i => i.ToString()).ToArray();
            
            var results = await cachedEcho(keys);

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
        public async Task WithReturnDictionaryFactory()
        {
            var count = 0;
            var dictionary = new Dictionary<string, string>();

            Func<IEqualityComparer<string>, int, IDictionary<string, string>> dictionaryFactoryFunc = (k, c) =>
            {
                count++;
                return dictionary;
            };
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithReturnDictionaryFactory(dictionaryFactoryFunc)
                    .Build();
            }

            var keys = new[] { "123", "234" };
            
            await cachedEcho(keys);

            count.Should().Be(1);
            dictionary.Should().ContainKeys(keys);
        }
    }
}