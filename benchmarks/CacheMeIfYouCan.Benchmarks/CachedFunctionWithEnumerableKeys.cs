using System;
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
        private readonly Func<List<int>, ValueTask<Dictionary<int, int>>> _func;
        private readonly List<int> _oneHit = Enumerable.Range(0, 1).ToList();
        private readonly List<int> _oneMiss = Enumerable.Range(100, 1).ToList();
        private readonly List<int> _oneHitAndOneMiss = Enumerable.Range(99, 2).ToList();
        private readonly List<int> _oneHundredHits = Enumerable.Range(0, 100).ToList();
        private readonly List<int> _oneHundredMisses = Enumerable.Range(100, 100).ToList();
        private readonly List<int> _oneHundredHitsAndOneHundredMisses = Enumerable.Range(0, 200).ToList();
        private readonly ValueTask<Dictionary<int, int>> _oneHitResults;
        private readonly ValueTask<Dictionary<int, int>> _oneMissResults;
        private readonly ValueTask<Dictionary<int, int>> _oneHitAndOneMissResults;
        private readonly ValueTask<Dictionary<int, int>> _oneHundredHitsResults;
        private readonly ValueTask<Dictionary<int, int>> _oneHundredMissesResults;
        private readonly ValueTask<Dictionary<int, int>> _oneHundredHitsAndOneHundredMissesResults;

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
                .ConfigureFor((List<int> x) => OriginalFunc(x))
                .WithEnumerableKeys<List<int>, Dictionary<int, int>, int, int>()
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
            return list.Count switch
            {
                1 => (list[0] == 0 ? _oneHitResults : _oneMissResults),
                2 => _oneHitAndOneMissResults,
                100 => (list[0] == 0 ? _oneHundredHitsResults : _oneHundredMissesResults),
                _ => _oneHundredHitsAndOneHundredMissesResults
            };
        }
        
        private static ValueTask<Dictionary<int, int>> BuildResults(List<int> list)
        {
            var values = new Dictionary<int, int>(list.Count);
            foreach (var i in list)
                values.Add(i, i);
            
            return new ValueTask<Dictionary<int, int>>(values);
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

|                            Method |        Mean |       Error |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|------------:|----------:|-------:|-------:|------:|----------:|
|                            OneHit |    661.3 ns |    26.55 ns |  24.83 ns | 0.0458 |      - |     - |     288 B |
|                           OneMiss |  1,275.5 ns |    11.57 ns |  10.82 ns | 0.0687 | 0.0172 |     - |     432 B |
|                  OneHitAndOneMiss |  1,380.0 ns |    10.72 ns |  10.02 ns | 0.0687 | 0.0172 |     - |     432 B |
|                    OneHundredHits |  3,424.3 ns |    38.47 ns |  35.98 ns | 0.3738 | 0.0038 |     - |    2368 B |
|                  OneHundredMisses | 24,222.4 ns | 1,091.25 ns | 967.36 ns | 1.7090 | 0.5493 |     - |   10882 B |
| OneHundredHitsAndOneHundredMisses | 27,798.6 ns |   871.19 ns | 772.29 ns | 1.2207 | 0.0305 |     - |    7704 B |
*/
