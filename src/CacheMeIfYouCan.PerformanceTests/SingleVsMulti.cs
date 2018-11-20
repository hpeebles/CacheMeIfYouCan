using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CacheMeIfYouCan.Caches;

namespace CacheMeIfYouCan.PerformanceTests
{
    public class SingleVsMulti<T>
    {
        private readonly IList<T> _keys;
        private readonly Func<T, Task<T>> _single;
        private readonly Func<IList<T>, Task<IDictionary<T, T>>> _multi;
        
        public SingleVsMulti()
        {
            _keys = GenerateKeys(1000);
            _single = BuildSingle();
            _multi = BuildMulti();
        }

        [Benchmark]
        public Task Single() => RunSingleTest(_single);

        [Benchmark]
        public Task Multi() => RunMultiTest(_multi);

        [Benchmark(Baseline = true)]
        public Task Baseline() => RunSingleTest(Task.FromResult);
        
        private async Task RunSingleTest(Func<T, Task<T>> func)
        {
            foreach (var key in _keys)
                await func(key);
        }

        private async Task RunMultiTest(Func<IList<T>, Task<IDictionary<T, T>>> func)
        {
            await func(_keys);
        }
        
        private static Func<T, Task<T>> BuildSingle()
        {
            Func<T, Task<T>> func = DummyFunc;
            
            return func
                .Cached()
                .For(TimeSpan.FromMinutes(10))
                .WithLocalCacheFactory(new DictionaryCacheFactory())
                .Build();
        }

        private static Func<IList<T>, Task<IDictionary<T, T>>> BuildMulti()
        {
            Func<IList<T>, Task<Dictionary<T, T>>> func = DummyMultiFunc;
            
            return func
                .Cached()
                .For(TimeSpan.FromMinutes(10))
                .WithLocalCacheFactory(new DictionaryCacheFactory())
                .Build();
        }

        private static IList<T> GenerateKeys(int count)
        {
            var generateKeyFunc = GenerateKeyFunc();
            
            return Enumerable
                .Range(0, count)
                .Select(generateKeyFunc)
                .ToArray();
        }

        private static Func<int, T> GenerateKeyFunc()
        {
            if (typeof(T) == typeof(int))
                return i => (T)(object)i;
            
            if (typeof(T) == typeof(string))
                return i => (T)(object)Guid.NewGuid().ToString();
            
            if (typeof(T) == typeof(Guid))
                return i => (T)(object)Guid.NewGuid();

            throw new Exception();
        }
        
        private static Task<T> DummyFunc(T key)
        {
            return Task.FromResult(key);
        }
        
        private static Task<Dictionary<T, T>> DummyMultiFunc(IList<T> keys)
        {
            return Task.FromResult(keys.ToDictionary(k => k));
        }
    }
}