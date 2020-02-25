using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithEnumerableKeys
    {
        private readonly Func<List<int>, Task<Dictionary<int, int>>> _func;
        private readonly List<int> _oneHit = Enumerable.Range(0, 1).ToList();
        private readonly List<int> _oneMiss = Enumerable.Range(100, 1).ToList();
        private readonly List<int> _oneHitAndOneMiss = Enumerable.Range(99, 2).ToList();
        private readonly List<int> _oneHundredHits = Enumerable.Range(0, 100).ToList();
        private readonly List<int> _oneHundredMisses = Enumerable.Range(100, 100).ToList();
        private readonly List<int> _oneHundredHitsAndOneHundredMisses = Enumerable.Range(0, 200).ToList();
        private readonly Task<Dictionary<int, int>> _oneHitResults;
        private readonly Task<Dictionary<int, int>> _oneMissResults;
        private readonly Task<Dictionary<int, int>> _oneHitAndOneMissResults;
        private readonly Task<Dictionary<int, int>> _oneHundredHitsResults;
        private readonly Task<Dictionary<int, int>> _oneHundredMissesResults;
        private readonly Task<Dictionary<int, int>> _oneHundredHitsAndOneHundredMissesResults;

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
                .ConfigureFor<List<int>, Dictionary<int, int>, int, int>(OriginalFunc)
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
        public Task<Dictionary<int, int>> OneHit() => _func(_oneHit);
        
        [Benchmark]
        public Task<Dictionary<int, int>> OneMiss() => _func(_oneMiss);
        
        [Benchmark]
        public Task<Dictionary<int, int>> OneHitAndOneMiss() => _func(_oneHitAndOneMiss);
        
        [Benchmark]
        public Task<Dictionary<int, int>> OneHundredHits() => _func(_oneHundredHits);
        
        [Benchmark]
        public Task<Dictionary<int, int>> OneHundredMisses() => _func(_oneHundredMisses);
        
        [Benchmark]
        public Task<Dictionary<int, int>> OneHundredHitsAndOneHundredMisses() => _func(_oneHundredHitsAndOneHundredMisses);

        private Task<Dictionary<int, int>> OriginalFunc(List<int> list)
        {
            return list.Count switch
            {
                1 => (list[0] == 0 ? _oneHitResults : _oneMissResults),
                2 => _oneHitAndOneMissResults,
                100 => (list[0] == 0 ? _oneHundredHitsResults : _oneHundredMissesResults),
                _ => _oneHundredHitsAndOneHundredMissesResults
            };
        }
        
        private static Task<Dictionary<int, int>> BuildResults(List<int> list)
        {
            var values = new Dictionary<int, int>(list.Count);
            foreach (var i in list)
                values.Add(i, i);
            
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


|                            Method |        Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|-------:|-------:|------:|----------:|
|                            OneHit |    385.7 ns |   7.65 ns |   7.86 ns | 0.0634 |      - |     - |     400 B |
|                           OneMiss |    942.5 ns |  18.50 ns |  16.40 ns | 0.0954 | 0.0229 |     - |     600 B |
|                  OneHitAndOneMiss |  1,064.2 ns |   3.21 ns |   2.50 ns | 0.1106 | 0.0267 |     - |     704 B |
|                    OneHundredHits |  9,759.8 ns | 194.08 ns | 265.66 ns | 0.3815 |      - |     - |    2480 B |
|                  OneHundredMisses | 36,653.2 ns | 711.71 ns | 791.07 ns | 1.8311 |      - |     - |   10968 B |
| OneHundredHitsAndOneHundredMisses | 49,104.1 ns | 875.97 ns | 776.53 ns | 1.7700 | 0.4272 |     - |   11176 B |
*/