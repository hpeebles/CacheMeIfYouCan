using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using Xunit;

namespace CacheMeIfYouCan.Tests.FunctionCache
{
    public class MultiKey : CacheTestBase
    {
        [Fact]
        public void MultiKeyCacheGetsBuilt()
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
            
            var multiType = typeof(MultiKeyFunctionCacheConfigurationManager<string, string>);
            
            Assert.IsType(multiType, a1.Cached());
            Assert.IsType(multiType, a2.Cached());
            Assert.IsType(multiType, a3.Cached());
            Assert.IsType(multiType, a4.Cached());

            Assert.IsType(multiType, b1.Cached());
            Assert.IsType(multiType, b2.Cached());
            Assert.IsType(multiType, b3.Cached());
            Assert.IsType(multiType, b4.Cached());
            
            Assert.IsType(multiType, c1.Cached());
            Assert.IsType(multiType, c2.Cached());
            Assert.IsType(multiType, c3.Cached());
            Assert.IsType(multiType, c4.Cached());
            
            Assert.IsType(multiType, d1.Cached());
            Assert.IsType(multiType, d2.Cached());
            Assert.IsType(multiType, d3.Cached());
            Assert.IsType(multiType, d4.Cached());
            
            Assert.IsType(multiType, e1.Cached());
            Assert.IsType(multiType, e2.Cached());
            Assert.IsType(multiType, e3.Cached());
            Assert.IsType(multiType, e4.Cached());
            
            Assert.IsType(multiType, f1.Cached());
            Assert.IsType(multiType, f2.Cached());
            Assert.IsType(multiType, f3.Cached());
            Assert.IsType(multiType, f4.Cached());
            
            Assert.IsType(multiType, g1.Cached());
            Assert.IsType(multiType, g2.Cached());
            Assert.IsType(multiType, g3.Cached());
            Assert.IsType(multiType, g4.Cached());
            
            Assert.IsType(multiType, h1.Cached());
            Assert.IsType(multiType, h2.Cached());
            Assert.IsType(multiType, h3.Cached());
            Assert.IsType(multiType, h4.Cached());
            
            Assert.IsType(multiType, i1.Cached());
            Assert.IsType(multiType, i2.Cached());
            Assert.IsType(multiType, i3.Cached());
            Assert.IsType(multiType, i4.Cached());
            
            Assert.IsType(multiType, j1.Cached());
            Assert.IsType(multiType, j2.Cached());
            Assert.IsType(multiType, j3.Cached());
            Assert.IsType(multiType, j4.Cached());
            
            Assert.IsType(multiType, k1.Cached());
            Assert.IsType(multiType, k2.Cached());
            Assert.IsType(multiType, k3.Cached());
            Assert.IsType(multiType, k4.Cached());
            
            Assert.IsType(multiType, l1.Cached());
            Assert.IsType(multiType, l2.Cached());
            Assert.IsType(multiType, l3.Cached());
            Assert.IsType(multiType, l4.Cached());
            
            Assert.IsType(multiType, m1.Cached());
            Assert.IsType(multiType, m2.Cached());
            Assert.IsType(multiType, m3.Cached());
            Assert.IsType(multiType, m4.Cached());
            
            Assert.IsType(multiType, n1.Cached());
            Assert.IsType(multiType, n2.Cached());
            Assert.IsType(multiType, n3.Cached());
            Assert.IsType(multiType, n4.Cached());
            
            Assert.IsType(multiType, o1.Cached());
            Assert.IsType(multiType, o2.Cached());
            Assert.IsType(multiType, o3.Cached());
            Assert.IsType(multiType, o4.Cached());
            
            Assert.IsType(multiType, p1.Cached());
            Assert.IsType(multiType, p2.Cached());
            Assert.IsType(multiType, p3.Cached());
            Assert.IsType(multiType, p4.Cached());
        }
        
        [Fact]
        public async Task OnlyFetchKeysWhichAreNotAlreadyCached()
        {
            FunctionCacheGetResult mostRecentResult = null;
            FunctionCacheFetchResult mostRecentFetch = null;
            
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> echo = new MultiEcho();
            Func<IEnumerable<string>, Task<IDictionary<string, string>>> cachedEcho;
            using (EnterSetup(false))
            {
                cachedEcho = echo
                    .Cached()
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
                
                Assert.Equal(keys.Length, mostRecentResult.Results.Count());
                Assert.Equal(Outcome.Fetch, mostRecentResult.Results.Last().Outcome);
                Assert.All(mostRecentResult.Results.SkipLast(1), x => Assert.Equal(Outcome.FromCache, x.Outcome));

                Assert.Single(mostRecentFetch.Results);
                Assert.Equal(i.ToString(), mostRecentFetch.Results.Single().KeyString);
            }
        }
    }
}