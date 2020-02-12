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

        public CachedFunctionWithEnumerableKeys()
        {
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
                .With(MemoryDiagnoser.Default)
                .With(Job.Default.With(new GcMode { Force = false })));
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

        private static Task<List<KeyValuePair<int, int>>> OriginalFunc(List<int> list)
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
  Job-TRYGSH : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Force=False

|                            Method |        Mean |       Error |      StdDev |      Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|------------:|------------:|------------:|-------:|-------:|------:|----------:|
|                            OneHit |    405.7 ns |     8.14 ns |    21.74 ns |    396.4 ns | 0.0892 |      - |     - |     560 B |
|                           OneMiss |    934.9 ns |    18.47 ns |    15.42 ns |    933.6 ns | 0.1240 | 0.0305 |     - |     784 B |
|                  OneHitAndOneMiss |  1,136.1 ns |    42.72 ns |    41.96 ns |  1,120.5 ns | 0.1507 | 0.0381 |     - |     952 B |
|                    OneHundredHits | 11,577.5 ns |    63.07 ns |    55.91 ns | 11,577.3 ns | 0.8698 |      - |     - |    5536 B |
|                  OneHundredMisses | 45,794.6 ns | 2,881.77 ns | 8,360.53 ns | 40,161.3 ns | 2.0752 | 0.1221 |     - |   13233 B |
| OneHundredHitsAndOneHundredMisses | 61,323.8 ns | 2,366.91 ns | 6,866.83 ns | 58,131.4 ns | 2.5024 | 0.6104 |     - |   15904 B |
*/