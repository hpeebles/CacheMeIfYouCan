using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.PerformanceTests
{
    public class SingleVsEnumerable<T>
    {
        private readonly IList<T> _keys;
        private readonly Func<T, Task<T>> _single;
        private readonly Func<IList<T>, Task<Dictionary<T, T>>> _enumerable;
        
        public SingleVsEnumerable()
        {
            _keys = KeyGenerator.Generate<T>(1000);
            _single = BuildSingle();
            _enumerable = BuildEnumerable();
        }

        [Benchmark]
        public Task Single() => RunSingleTest(_single);

        [Benchmark]
        public Task Enumerable() => RunEnumerableTest(_enumerable);

        [Benchmark(Baseline = true)]
        public Task Baseline() => RunSingleTest(Task.FromResult);
        
        private async Task RunSingleTest(Func<T, Task<T>> func)
        {
            foreach (var key in _keys)
                await func(key);
        }

        private async Task RunEnumerableTest(Func<IList<T>, Task<Dictionary<T, T>>> func)
        {
            await func(_keys);
        }
        
        private static Func<T, Task<T>> BuildSingle()
        {
            Func<T, Task<T>> func = DummyFunc;
            
            return func
                .Cached()
                .WithTimeToLive(TimeSpan.FromMinutes(10))
                .WithLocalCacheFactory(new DictionaryCacheFactory())
                .Build();
        }

        private static Func<IList<T>, Task<Dictionary<T, T>>> BuildEnumerable()
        {
            Func<IList<T>, Task<Dictionary<T, T>>> func = DummyEnumerableFunc;
            
            return func
                .Cached<IList<T>, Dictionary<T, T>, T, T>()
                .WithTimeToLive(TimeSpan.FromMinutes(10))
                .WithLocalCacheFactory(new DictionaryCacheFactory())
                .Build();
        }

        private static Task<T> DummyFunc(T key)
        {
            return Task.FromResult(key);
        }
        
        private static Task<Dictionary<T, T>> DummyEnumerableFunc(IList<T> keys)
        {
            return Task.FromResult(keys.ToDictionary(k => k));
        }
    }
}