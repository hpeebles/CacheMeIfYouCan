using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithEnumerableKeys
    {
        private readonly Func<List<int>, Task<List<KeyValuePair<int, int>>>> _func;
        private readonly List<int> _oneHit = Enumerable.Range(0, 1).ToList();
        private readonly List<int> _oneMiss = Enumerable.Range(100, 1).ToList();
        private readonly List<int> _oneHitAndOneMiss = Enumerable.Range(99, 2).ToList();
        private readonly List<int> _oneHundredHits = Enumerable.Range(0, 100).ToList();
        private readonly List<int> _oneHundredMisses = Enumerable.Range(100, 100).ToList();
        private readonly List<int> _oneHundredHitsAndOneHundredMisses = Enumerable.Range(0, 200).ToList();
        private readonly Task<List<KeyValuePair<int, int>>> _oneHitResults;
        private readonly Task<List<KeyValuePair<int, int>>> _oneMissResults;
        private readonly Task<List<KeyValuePair<int, int>>> _oneHitAndOneMissResults;
        private readonly Task<List<KeyValuePair<int, int>>> _oneHundredHitsResults;
        private readonly Task<List<KeyValuePair<int, int>>> _oneHundredMissesResults;
        private readonly Task<List<KeyValuePair<int, int>>> _oneHundredHitsAndOneHundredMissesResults;

        public CachedFunctionWithEnumerableKeys()
        {
            _oneHitResults = BuildResults(_oneHit);
            _oneMissResults = BuildResults(_oneMiss);
            _oneHitAndOneMissResults = BuildResults(_oneHitAndOneMiss);
            _oneHundredHitsResults = BuildResults(_oneHundredHits);
            _oneHundredMissesResults = BuildResults(_oneHundredMisses);
            _oneHundredHitsAndOneHundredMissesResults = BuildResults(_oneHundredHitsAndOneHundredMisses);
            
            var cache = new DictionaryCache<int, int>();
            for (var i = 0; i < 100; i++)
                cache.Set(i, i, TimeSpan.FromHours(1));
            
            _func = CachedFunctionFactory
                .ConfigureFor<List<int>, List<KeyValuePair<int, int>>, int, int>(OriginalFunc)
                .WithTimeToLive(TimeSpan.FromTicks(1))
                .WithLocalCache(cache)
                .Build();
        }
        
        public static void Run()
        {
#if DEBUG
            var runner = new CachedFunctionWithEnumerableKeys();
            
            runner.OneHit();
            runner.OneMiss();
            runner.OneHitAndOneMiss();
            runner.OneHundredHits();
            runner.OneHundredMisses();
            runner.OneHundredHitsAndOneHundredMisses();
#else
            BenchmarkRunner.Run<CachedFunctionWithEnumerableKeys>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default));
#endif
        }

        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneHit() => _func(_oneHit);
        
        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneMiss() => _func(_oneMiss);
        
        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneHitAndOneMiss() => _func(_oneHitAndOneMiss);
        
        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneHundredHits() => _func(_oneHundredHits);
        
        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneHundredMisses() => _func(_oneHundredMisses);
        
        [Benchmark]
        public Task<List<KeyValuePair<int, int>>> OneHundredHitsAndOneHundredMisses() => _func(_oneHundredHitsAndOneHundredMisses);

        private Task<List<KeyValuePair<int, int>>> OriginalFunc(List<int> list)
        {
            return list.Count switch
            {
                1 => (list[0] == 0 ? _oneHitResults : _oneMissResults),
                2 => _oneHitAndOneMissResults,
                100 => (list[0] == 0 ? _oneHundredHitsResults : _oneHundredMissesResults),
                _ => _oneHundredHitsAndOneHundredMissesResults
            };
        }
        
        private static Task<List<KeyValuePair<int, int>>> BuildResults(List<int> list)
        {
            var values = new List<KeyValuePair<int, int>>(list.Count);
            foreach (var i in list)
                values.Add(new KeyValuePair<int, int>(i, i));
            
            return Task.FromResult(values);
        }
    }
}

/*
// * Summary *

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


|                            Method |        Mean |     Error |      StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|------------:|-------:|-------:|------:|----------:|
|                            OneHit |    412.5 ns |   8.22 ns |     8.07 ns | 0.0892 |      - |     - |     560 B |
|                           OneMiss |    891.3 ns |  17.15 ns |    16.04 ns | 0.1030 | 0.0257 |     - |     648 B |
|                  OneHitAndOneMiss |  1,076.7 ns |  11.49 ns |     9.60 ns | 0.1297 | 0.0324 |     - |     816 B |
|                    OneHundredHits | 12,235.5 ns | 156.05 ns |   145.96 ns | 0.8698 |      - |     - |    5536 B |
|                  OneHundredMisses | 37,087.0 ns | 728.67 ns | 1,134.45 ns | 1.9531 | 0.0610 |     - |   12603 B |
| OneHundredHitsAndOneHundredMisses | 48,958.5 ns | 960.44 ns |   851.40 ns | 2.3804 | 0.9155 |     - |   14976 B |
*/