using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace CacheMeIfYouCan.Benchmarks
{
    public class CachedFunctionWithSingleKey
    {
        private readonly Func<int, ValueTask<int>> _func;

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
                .With(Job.MediumRun.WithLaunchCount(1))
                .With(MemoryDiagnoser.Default));
#endif
        }

        [Benchmark]
        public ValueTask<int> CacheHit() => _func(1);
        
        [Benchmark]
        public ValueTask<int> CacheMiss() => _func(2);

        private static ValueTask<int> OriginalFunc(int k) => new ValueTask<int>(k);
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

|    Method |      Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |----------:|---------:|---------:|------:|------:|------:|----------:|
|  CacheHit |  90.10 ns | 0.269 ns | 0.252 ns |     - |     - |     - |         - |
| CacheMiss | 180.04 ns | 1.192 ns | 1.115 ns |     - |     - |     - |         - |
*/