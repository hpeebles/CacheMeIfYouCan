using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Tests.Proxy;

namespace CacheMeIfYouCan.PerformanceTests
{
    public class CacheComparison
    {
        private readonly IList<string> _keys;
        private readonly ITest _v1;
        private readonly ITest _v2;

        public CacheComparison()
        {
            _keys = KeyGenerator.Generate<string>(100000);
            
            var testImpl = new TestImpl();

            _v1 = testImpl.Cached<ITest>().WithLocalCacheFactory(new MemoryCacheFactory()).WithTimeToLive(TimeSpan.FromMinutes(10)).Build();
            _v2 = testImpl.Cached<ITest>().WithLocalCacheFactory(new DictionaryCacheFactory()).WithTimeToLive(TimeSpan.FromMinutes(10)).Build();
        }

        [Benchmark]
        public Task V1() => RunTest(_v1);

        [Benchmark]
        public Task V2() => RunTest(_v2);
        
        private async Task RunTest(ITest impl)
        {
            foreach (var key in _keys)
                await impl.StringToString(key);
        }
    }
}