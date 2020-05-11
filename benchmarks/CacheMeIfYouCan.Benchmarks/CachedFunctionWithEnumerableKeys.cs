﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithEnumerableKeys
    {
        private readonly Func<IList<int>, ValueTask<Dictionary<int, int>>> _func;
        private readonly IList<int> _oneHit = Enumerable.Range(0, 1).ToArray();
        private readonly IList<int> _oneMiss = Enumerable.Range(100, 1).ToArray();
        private readonly IList<int> _oneHitAndOneMiss = Enumerable.Range(99, 2).ToArray();
        private readonly IList<int> _oneHundredHits = Enumerable.Range(0, 100).ToArray();
        private readonly IList<int> _oneHundredMisses = Enumerable.Range(100, 100).ToArray();
        private readonly IList<int> _oneHundredHitsAndOneHundredMisses = Enumerable.Range(0, 200).ToArray();
        private readonly Dictionary<int, int> _oneHitResults;
        private readonly Dictionary<int, int> _oneMissResults;
        private readonly Dictionary<int, int> _oneHitAndOneMissResults;
        private readonly Dictionary<int, int> _oneHundredHitsResults;
        private readonly Dictionary<int, int> _oneHundredMissesResults;
        private readonly Dictionary<int, int> _oneHundredHitsAndOneHundredMissesResults;

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
                .ConfigureFor((IList<int> x) => OriginalFunc(x))
                .WithEnumerableKeys<IList<int>, Dictionary<int, int>, int, int>()
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
                .With(Job.MediumRun.WithLaunchCount(1))
                .With(MemoryDiagnoser.Default));
#endif
        }
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneHit() => _func(_oneHit);
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneMiss() => _func(_oneMiss);
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneHitAndOneMiss() => _func(_oneHitAndOneMiss);
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneHundredHits() => _func(_oneHundredHits);
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneHundredMisses() => _func(_oneHundredMisses);
        
        [Benchmark]
        public ValueTask<Dictionary<int, int>> OneHundredHitsAndOneHundredMisses() => _func(_oneHundredHitsAndOneHundredMisses);

        private ValueTask<Dictionary<int, int>> OriginalFunc(IList<int> list)
        {
            var dictionary = list.Count switch
            {
                1 => (list[0] == 0 ? _oneHitResults : _oneMissResults),
                2 => _oneHitAndOneMissResults,
                100 => (list[0] == 0 ? _oneHundredHitsResults : _oneHundredMissesResults),
                _ => _oneHundredHitsAndOneHundredMissesResults
            };
            
            return new ValueTask<Dictionary<int, int>>(new Dictionary<int, int>(dictionary));
        }
        
        private static Dictionary<int, int> BuildResults(IList<int> list)
        {
            var values = new Dictionary<int, int>(list.Count);
            foreach (var i in list)
                values.Add(i, i);
            
            return values;
        }
    }
}

/*
// * Summary *

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]    : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  MediumRun : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Job=MediumRun  IterationCount=15  LaunchCount=1
WarmupCount=10

|                            Method |        Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|-------:|-------:|------:|----------:|
|                            OneHit |    437.7 ns |   5.68 ns |   5.32 ns | 0.0391 |      - |     - |     248 B |
|                           OneMiss |    914.4 ns |  10.82 ns |   9.59 ns | 0.0467 | 0.0086 |     - |     298 B |
|                  OneHitAndOneMiss |    996.7 ns |  14.98 ns |  14.01 ns | 0.0782 | 0.0191 |     - |     496 B |
|                    OneHundredHits |  2,613.0 ns |  18.27 ns |  16.19 ns | 0.3700 | 0.0038 |     - |    2328 B |
|                  OneHundredMisses | 21,696.7 ns | 982.24 ns | 870.73 ns | 0.8240 | 0.2747 |     - |    5302 B |
| OneHundredHitsAndOneHundredMisses | 26,992.5 ns | 996.80 ns | 883.64 ns | 1.9531 | 0.8240 |     - |   12296 B |
*/
