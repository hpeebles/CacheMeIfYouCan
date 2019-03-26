using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Tests.Common;
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
            
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, IDictionary<string, string>, string, string>>(a1.Cached<IEnumerable<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, Dictionary<string, string>, string, string>>(a2.Cached<IEnumerable<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>>(a3.Cached<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IEnumerable<string>, SortedDictionary<string, string>, string, string>>(a4.Cached<IEnumerable<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IEnumerable<string>, IDictionary<string, string>, string, string>>(b1.Cached<IEnumerable<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IEnumerable<string>, Dictionary<string, string>, string, string>>(b2.Cached<IEnumerable<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>>(b3.Cached<IEnumerable<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IEnumerable<string>, SortedDictionary<string, string>, string, string>>(b4.Cached<IEnumerable<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IList<string>, IDictionary<string, string>, string, string>>(c1.Cached<IList<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IList<string>, Dictionary<string, string>, string, string>>(c2.Cached<IList<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IList<string>, ConcurrentDictionary<string, string>, string, string>>(c3.Cached<IList<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<IList<string>, SortedDictionary<string, string>, string, string>>(c4.Cached<IList<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IList<string>, IDictionary<string, string>, string, string>>(d1.Cached<IList<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IList<string>, Dictionary<string, string>, string, string>>(d2.Cached<IList<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IList<string>, ConcurrentDictionary<string, string>, string, string>>(d3.Cached<IList<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<IList<string>, SortedDictionary<string, string>, string, string>>(d4.Cached<IList<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<List<string>, IDictionary<string, string>, string, string>>(e1.Cached<List<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<List<string>, Dictionary<string, string>, string, string>>(e2.Cached<List<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<List<string>, ConcurrentDictionary<string, string>, string, string>>(e3.Cached<List<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<List<string>, SortedDictionary<string, string>, string, string>>(e4.Cached<List<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<List<string>, IDictionary<string, string>, string, string>>(f1.Cached<List<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<List<string>, Dictionary<string, string>, string, string>>(f2.Cached<List<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<List<string>, ConcurrentDictionary<string, string>, string, string>>(f3.Cached<List<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<List<string>, SortedDictionary<string, string>, string, string>>(f4.Cached<List<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<string[], IDictionary<string, string>, string, string>>(g1.Cached<string[], IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<string[], Dictionary<string, string>, string, string>>(g2.Cached<string[], Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<string[], ConcurrentDictionary<string, string>, string, string>>(g3.Cached<string[], ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<string[], SortedDictionary<string, string>, string, string>>(g4.Cached<string[], SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<string[], IDictionary<string, string>, string, string>>(h1.Cached<string[], IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<string[], Dictionary<string, string>, string, string>>(h2.Cached<string[], Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<string[], ConcurrentDictionary<string, string>, string, string>>(h3.Cached<string[], ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<string[], SortedDictionary<string, string>, string, string>>(h4.Cached<string[], SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ICollection<string>, IDictionary<string, string>, string, string>>(i1.Cached<ICollection<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ICollection<string>, Dictionary<string, string>, string, string>>(i2.Cached<ICollection<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ICollection<string>, ConcurrentDictionary<string, string>, string, string>>(i3.Cached<ICollection<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ICollection<string>, SortedDictionary<string, string>, string, string>>(i4.Cached<ICollection<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ICollection<string>, IDictionary<string, string>, string, string>>(j1.Cached<ICollection<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ICollection<string>, Dictionary<string, string>, string, string>>(j2.Cached<ICollection<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ICollection<string>, ConcurrentDictionary<string, string>, string, string>>(j3.Cached<ICollection<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ICollection<string>, SortedDictionary<string, string>, string, string>>(j4.Cached<ICollection<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ISet<string>, IDictionary<string, string>, string, string>>(k1.Cached<ISet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ISet<string>, Dictionary<string, string>, string, string>>(k2.Cached<ISet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ISet<string>, ConcurrentDictionary<string, string>, string, string>>(k3.Cached<ISet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<ISet<string>, SortedDictionary<string, string>, string, string>>(k4.Cached<ISet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ISet<string>, IDictionary<string, string>, string, string>>(l1.Cached<ISet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ISet<string>, Dictionary<string, string>, string, string>>(l2.Cached<ISet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ISet<string>, ConcurrentDictionary<string, string>, string, string>>(l3.Cached<ISet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<ISet<string>, SortedDictionary<string, string>, string, string>>(l4.Cached<ISet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<HashSet<string>, IDictionary<string, string>, string, string>>(m1.Cached<HashSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<HashSet<string>, Dictionary<string, string>, string, string>>(m2.Cached<HashSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<HashSet<string>, ConcurrentDictionary<string, string>, string, string>>(m3.Cached<HashSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<HashSet<string>, SortedDictionary<string, string>, string, string>>(m4.Cached<HashSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<HashSet<string>, IDictionary<string, string>, string, string>>(n1.Cached<HashSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<HashSet<string>, Dictionary<string, string>, string, string>>(n2.Cached<HashSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<HashSet<string>, ConcurrentDictionary<string, string>, string, string>>(n3.Cached<HashSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<HashSet<string>, SortedDictionary<string, string>, string, string>>(n4.Cached<HashSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<SortedSet<string>, IDictionary<string, string>, string, string>>(o1.Cached<SortedSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<SortedSet<string>, Dictionary<string, string>, string, string>>(o2.Cached<SortedSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>>(o3.Cached<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerNoCanx<SortedSet<string>, SortedDictionary<string, string>, string, string>>(o4.Cached<SortedSet<string>, SortedDictionary<string, string>, string, string>());

            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<SortedSet<string>, IDictionary<string, string>, string, string>>(p1.Cached<SortedSet<string>, IDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<SortedSet<string>, Dictionary<string, string>, string, string>>(p2.Cached<SortedSet<string>, Dictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>>(p3.Cached<SortedSet<string>, ConcurrentDictionary<string, string>, string, string>());
            Assert.IsType<EnumerableKeyFunctionCacheConfigurationManagerSyncNoCanx<SortedSet<string>, SortedDictionary<string, string>, string, string>>(p4.Cached<SortedSet<string>, SortedDictionary<string, string>, string, string>());
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
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CatchDuplicateRequests(bool catchDuplicateRequests)
        {
            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();

            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.FromSeconds(1));
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .CatchDuplicateRequests(catchDuplicateRequests)
                    .OnFetch(fetches.Add)
                    .Build();
            }
            
            await cachedEcho(new[] { "warmup" });

            var tasks = Enumerable
                .Range(0, 5)
                .Select(i => cachedEcho(new[] { "123" }))
                .ToArray();

            await Task.WhenAll(tasks);

            fetches.Should().HaveCount(6);

            fetches
                .SelectMany(f => f.Results)
                .Count(f => f.Duplicate)
                .Should()
                .Be(catchDuplicateRequests ? 4 : 0);
        }
        
        [Fact]
        public async Task WithBatchedFetches()
        {
            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();
            
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

        [Theory]
        [InlineData(BatchBehaviour.FillBatchesEvenly)]
        [InlineData(BatchBehaviour.FillEachBatchBeforeStartingNext)]
        public async Task SetBatchBehaviour(BatchBehaviour batchBehaviour)
        {
            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnFetch(fetches.Add)
                    .WithBatchedFetches(50, batchBehaviour)
                    .Build();
            }

            var keys = Enumerable.Range(0, 101).Select(i => i.ToString()).ToArray();
            
            var results = await cachedEcho(keys);

            results.Should().ContainKeys(keys);
            
            fetches.Should().HaveCount(3);

            if (batchBehaviour == BatchBehaviour.FillBatchesEvenly)
                fetches.Select(f => f.Results.Count).OrderBy(c => c).Should().BeEquivalentTo(new[] { 33, 34, 34 });
            else
                fetches.Select(f => f.Results.Count).OrderBy(c => c).Should().BeEquivalentTo(new[] { 1, 50, 50 });
        }

        [Fact]
        public void FillMissingKeys()
        {
            var results = new List<FunctionCacheGetResult>();
            var value = Guid.NewGuid().ToString();
            
            Func<IEnumerable<string>, IDictionary<string, string>> func = inputKeys =>
                inputKeys.Where(k => k != "2").ToDictionary(k => k, k => k);
            
            Func<IEnumerable<string>, IDictionary<string, string>> cachedFunc;
            using (_setupLock.Enter())
            {
                cachedFunc = func
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .OnResult(results.Add)
                    .FillMissingKeys(value)
                    .Build();
            }

            var keys = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();
            
            var fromFunc = cachedFunc(keys);

            fromFunc.Keys.Should().BeEquivalentTo(keys);
            fromFunc["2"].Should().Be(value);

            cachedFunc(keys);

            results.Should().HaveCount(2);
            results.Last().Results.Should().HaveCount(10);

            foreach (var result in results.Last().Results)
                result.Outcome.Should().Be(Outcome.FromCache);
        }

        [Fact]
        public async Task WithReturnDictionaryFactory()
        {
            var builder = new TestDictionaryBuilder(EqualityComparer<string>.Default);
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithReturnDictionaryBuilder(builder)
                    .Build();
            }

            var keys = new[] { "123", "234" };
            
            var results = await cachedEcho(keys);

            results.Should().ContainKeys(keys);
            builder.Count.Should().Be(1);
        }
        
        [Fact]
        public async Task WithJitter()
        {
            var fetches = new ConcurrentBag<FunctionCacheFetchResult>();

            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho(TimeSpan.FromSeconds(1));
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (_setupLock.Enter())
            {
                cachedEcho = echo
                    .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                    .WithTimeToLive(TimeSpan.FromSeconds(1), 50)
                    .OnFetch(fetches.Add)
                    .Build();
            }
            
            await cachedEcho(new[] { "warmup" });

            var keys = Enumerable
                .Range(0, 20)
                .Select(i => Guid.NewGuid().ToString())
                .ToArray();
            
            await Task.WhenAll(keys.Select(k => cachedEcho(new[] { k })));
            
            fetches.Should().HaveCount(21);

            await Task.Delay(TimeSpan.FromMilliseconds(750));
            
            await Task.WhenAll(keys.Select(k => cachedEcho(new[] { k })));

            fetches.Should().HaveCountGreaterThan(21).And.HaveCountLessThan(41);
        }

        private class TestDictionaryBuilder : ReturnDictionaryBuilder<string, string, IDictionary<string, string>>
        {
            public TestDictionaryBuilder(IEqualityComparer<string> keyComparer)
                : base(keyComparer)
            { }

            public int Count { get; private set; }

            protected override IDictionary<string, string> InitializeDictionary(
                IEqualityComparer<string> keyComparer, int count)
            {
                Count++;
                
                return new Dictionary<string, string>();
            }
        }
    }
}