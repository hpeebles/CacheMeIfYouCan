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
|                            OneHit |    432.5 ns |   1.11 ns |     0.98 ns | 0.0739 |      - |     - |     464 B |
|                           OneMiss |    956.2 ns |  13.06 ns |    11.58 ns | 0.1030 | 0.0248 |     - |     648 B |
|                  OneHitAndOneMiss |  1,120.7 ns |  25.12 ns |    25.80 ns | 0.1202 | 0.0305 |     - |     760 B |
|                    OneHundredHits | 10,158.8 ns |  71.56 ns |    63.44 ns | 0.5188 |      - |     - |    3336 B |
|                  OneHundredMisses | 36,477.7 ns | 812.64 ns | 1,112.36 ns | 1.8311 | 0.4883 |     - |   11808 B |
| OneHundredHitsAndOneHundredMisses | 48,382.9 ns | 953.71 ns |   979.38 ns | 2.0142 | 0.7324 |     - |   12816 B |
*/