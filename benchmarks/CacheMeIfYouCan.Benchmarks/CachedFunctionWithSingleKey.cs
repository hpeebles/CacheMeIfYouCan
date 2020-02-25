using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.LocalCaches;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithSingleKey
    {
        private readonly Func<int, Task<int>> _func;
        private readonly Task<int>[] _results = Enumerable.Range(1, 2).Select(Task.FromResult).ToArray();

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
                .With(MemoryDiagnoser.Default));
#endif
        }

        [Benchmark]
        public Task<int> CacheHit() => _func(1);
        
        [Benchmark]
        public Task<int> CacheMiss() => _func(2);

        private Task<int> OriginalFunc(int k) => _results[k - 1];
    }
}

/*
// * Summary *

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


|    Method |     Mean |    Error |   StdDev |   Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------- |---------:|---------:|---------:|---------:|-------:|-------:|------:|----------:|
|  CacheHit | 131.3 ns |  0.81 ns |  0.68 ns | 131.4 ns |      - |      - |     - |         - |
| CacheMiss | 415.7 ns | 31.35 ns | 92.45 ns | 364.3 ns | 0.0048 | 0.0014 |     - |      32 B |
*/