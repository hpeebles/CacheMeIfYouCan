using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithSingleKey
    {
        private readonly Func<int, Task<int>> _func;

        public CachedFunctionWithSingleKey()
        {
            var cache = new DictionaryCache<int, int>();
            cache.Set(1, 1, TimeSpan.FromHours(1));
            
            _func = CachedFunctionFactory
                .ConfigureFor<int, int>(OriginalFunc)
                .WithTimeToLive(TimeSpan.FromTicks(1))
                .WithLocalCache(cache)
                .Build();
        }
        
        public static void Run()
        {
#if DEBUG
            var runner = new CachedFunctionWithSingleKey();
            
            runner.CacheHit();
            runner.CacheMiss();
#else
            BenchmarkRunner.Run<CachedFunctionWithSingleKey>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default)
                .With(Job.Default.With(new GcMode { Force = false })));
#endif
        }

        [Benchmark]
        public Task<int> CacheHit() => _func(1);
        
        [Benchmark]
        public Task<int> CacheMiss() => _func(2);

        private static Task<int> OriginalFunc(int k) => Task.FromResult(k);
    }
}

/*
// * Summary *

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  Job-GBDCCG : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Force=False

|    Method |     Mean |    Error |    StdDev |   Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------- |---------:|---------:|----------:|---------:|-------:|-------:|------:|----------:|
|  CacheHit | 129.6 ns |  0.89 ns |   0.83 ns | 129.7 ns |      - |      - |     - |         - |
| CacheMiss | 555.6 ns | 40.48 ns | 118.72 ns | 482.5 ns | 0.0162 | 0.0038 |     - |     104 B |
*/