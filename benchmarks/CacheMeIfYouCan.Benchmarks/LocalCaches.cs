using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace CacheMeIfYouCan.Benchmarks
{
    public class LocalCaches
    {
        private readonly MemoryCache<int, int> _memoryCache;
        private readonly DictionaryCache<int, int> _dictionaryCache;
        private readonly ReadOnlyMemory<int> _getManyKeys = Enumerable.Range(0, 100).ToArray();
        private readonly Memory<KeyValuePair<int, int>> _getManyDestination = new KeyValuePair<int, int>[100];
        private readonly ReadOnlyMemory<KeyValuePair<int, int>> _setManyValues = Enumerable.Range(0, 100).Select(i => new KeyValuePair<int, int>(i, i)).ToArray();

        public LocalCaches()
        {
            _memoryCache = new MemoryCache<int, int>();
            _dictionaryCache = new DictionaryCache<int, int>();

            for (var i = 0; i < 100; i++)
            {
                _memoryCache.Set(i, i, TimeSpan.FromHours(1));
                _dictionaryCache.Set(i, i, TimeSpan.FromHours(1));
            }
        }

        public static void Run()
        {
#if DEBUG
            var runner = new LocalCaches();
            
            runner.MemoryCache_Get();
            runner.DictionaryCache_Get();
            runner.MemoryCache_Set();
            runner.DictionaryCache_Set();
            runner.MemoryCache_GetMany();
            runner.DictionaryCache_GetMany();
            runner.MemoryCache_SetMany();
            runner.DictionaryCache_SetMany();
#else
            BenchmarkRunner.Run<LocalCaches>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(Job.MediumRun.WithLaunchCount(1))
                .With(MemoryDiagnoser.Default));
#endif
        }

        [Benchmark]
        public int MemoryCache_Get() => _memoryCache.TryGet(1, out var value) ? value : 0;

        [Benchmark]
        public int DictionaryCache_Get() => _dictionaryCache.TryGet(1, out var value) ? value : 0;

        [Benchmark]
        public void MemoryCache_Set() => _memoryCache.Set(1, 1, TimeSpan.FromHours(1));

        [Benchmark]
        public void DictionaryCache_Set() => _dictionaryCache.Set(1, 1, TimeSpan.FromHours(1));
        
        [Benchmark]
        public int MemoryCache_GetMany() => _memoryCache.GetMany(_getManyKeys.Span, _getManyDestination.Span);

        [Benchmark]
        public int DictionaryCache_GetMany() => _dictionaryCache.GetMany(_getManyKeys.Span, _getManyDestination.Span);

        [Benchmark]
        public void MemoryCache_SetMany() => _memoryCache.SetMany(_setManyValues.Span, TimeSpan.FromHours(1));

        [Benchmark]
        public void DictionaryCache_SetMany() => _dictionaryCache.SetMany(_setManyValues.Span, TimeSpan.FromHours(1));
    }
}
/*
// * Summary *

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100-preview.5.20251.2
  [Host]    : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT
  MediumRun : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT

Job=MediumRun  IterationCount=15  LaunchCount=1
WarmupCount=10

|                  Method |         Mean |      Error |     StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------------ |-------------:|-----------:|-----------:|-------:|-------:|------:|----------:|
|         MemoryCache_Get |    188.56 ns |   1.996 ns |   1.770 ns | 0.0050 |      - |     - |      32 B |
|     DictionaryCache_Get |     15.71 ns |   0.146 ns |   0.137 ns |      - |      - |     - |         - |
|         MemoryCache_Set |  1,079.78 ns |   6.498 ns |   5.760 ns | 0.4263 | 0.0010 |     - |    2680 B |
|     DictionaryCache_Set |     74.78 ns |   0.306 ns |   0.286 ns |      - |      - |     - |         - |
|     MemoryCache_GetMany | 21,828.41 ns | 239.081 ns | 223.636 ns | 0.9460 |      - |     - |    6080 B |
| DictionaryCache_GetMany |  1,211.71 ns |  23.959 ns |  21.239 ns |      - |      - |     - |         - |
|     MemoryCache_SetMany | 70,520.24 ns | 408.412 ns | 382.029 ns | 7.2021 | 0.6104 |     - |   45280 B |
| DictionaryCache_SetMany |  7,043.26 ns |  15.640 ns |  14.630 ns |      - |      - |     - |         - |
*/