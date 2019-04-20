using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan;

namespace Benchmarks.FunctionCacheComparison
{
    /// <summary>
    /// By default all the function caches are using a <see cref="MemoryCache"/> under the hood.
    /// These tests show how much overhead each function cache adds on top of calling <see cref="MemoryCache"/> directly.
    /// </summary>
    public class FunctionCacheBenchmarksRunner
    {
        private readonly Func<string, string> _singleKey;
        private readonly Func<IEnumerable<string>, IDictionary<string, string>> _enumerableKey;
        private readonly Func<string, string, string> _multiParam;
        private readonly Func<string, IEnumerable<string>, Dictionary<string, string>> _multiParamEnumerableKey;
        private readonly MemoryCache _memoryCache;
        private readonly string _randomGuidAsString = Guid.NewGuid().ToString();
        private readonly IEnumerable<string> _keys;
        private const int KeyCount = 1;
        
        public FunctionCacheBenchmarksRunner()
        {
            _keys = Enumerable
                .Range(0, KeyCount)
                .Select(i => Guid.NewGuid().ToString())
                .ToArray();
            
            Func<string, string> singleKeyTemp = key => key + key;
            _singleKey = singleKeyTemp
                .Cached()
                .Build();
            
            Func<IEnumerable<string>, IDictionary<string, string>> enumerableKeyTemp = keys => keys.ToDictionary(k => k, k => k + k);
            _enumerableKey = enumerableKeyTemp
                .Cached<IEnumerable<string>, IDictionary<string, string>, string, string>()
                .Build();

            Func<string, string, string> multiParamTemp = (k1, k2) => k1 + k2;
            _multiParam = multiParamTemp
                .Cached()
                .Build();
            
            Func<string, IEnumerable<string>, Dictionary<string, string>> multiParamEnumerableKeyTemp = (outerKey, innerKeys) =>
                innerKeys.ToDictionary(k => k, k => outerKey + k);
            _multiParamEnumerableKey = multiParamEnumerableKeyTemp
                .Cached<string, IEnumerable<string>, Dictionary<string, string>, string, string>()
                .Build();
            
            _memoryCache = new MemoryCache("test");
        }
        
        public static void Run()
        {
#if DEBUG
            var runner = new FunctionCacheBenchmarksRunner();
            
            runner.Baseline();
            runner.SingleKey();
            runner.EnumerableKey();
            runner.MultiParam();
            runner.MultiParamEnumerableKey();
#else
            BenchmarkRunner.Run<FunctionCacheBenchmarksRunner>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default)
                .With(Job.Default.With(new GcMode { Force = false })));
#endif
        }
        
        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            foreach (var key in _keys)
            {
                var value = _memoryCache.Get(key);
                if (value == null)
                {
                    value = key + key;
                    _memoryCache.Set(key, value, DateTimeOffset.UtcNow.AddHours(1));
                }
            }
        }

        [Benchmark]
        public void SingleKey()
        {
            foreach (var key in _keys)
            {
                var value = _singleKey(key);
            }
        }
        
        [Benchmark]
        public void EnumerableKey()
        {
            var values = _enumerableKey(_keys);
        }
        
        [Benchmark]
        public void MultiParam()
        {
            foreach (var key in _keys)
            {
                var values = _multiParam(key, key);
            }
        }

        [Benchmark]
        public void MultiParamEnumerableKey()
        {
            var values = _multiParamEnumerableKey(_randomGuidAsString, _keys);
        }
    }
}

/*
BenchmarkDotNet=v0.11.3, OS=Windows 10.0.16299.611 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-4500U CPU 1.80GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
Frequency=2338337 Hz, Resolution=427.6544 ns, Timer=TSC
.NET Core SDK=2.2.102
  [Host]     : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT
  Job-XFLGJQ : .NET Core 2.1.7 (CoreCLR 4.6.27129.04, CoreFX 4.6.27129.04), 64bit RyuJIT


KeyCount = 1
                  Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------ |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
                Baseline |    16.98 ns |  0.2667 ns |  0.2494 ns |   1.00 |    0.00 |           - |           - |           - |                   - |
               SingleKey |    17.11 ns |  0.1724 ns |  0.1612 ns |   1.01 |    0.01 |           - |           - |           - |                   - |
           EnumerableKey | 5,221.23 ns | 74.6327 ns | 69.8115 ns | 307.54 |    5.68 |      0.3662 |           - |           - |               256 B |
              MultiParam |    17.03 ns |  0.1828 ns |  0.1621 ns |   1.00 |    0.01 |           - |           - |           - |                   - |
 MultiParamEnumerableKey | 6,515.01 ns | 85.3741 ns | 75.6819 ns | 383.33 |    5.34 |      0.4807 |           - |           - |               264 B |


KeyCount = 10
                  Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                Baseline |  2.216 us | 0.0329 us | 0.0275 us |  1.00 |    0.00 |      0.1640 |           - |           - |               352 B |
               SingleKey | 64.841 us | 1.4050 us | 1.2455 us | 29.30 |    0.67 |      2.8076 |           - |           - |              2592 B |
           EnumerableKey | 55.349 us | 0.9082 us | 0.8495 us | 24.97 |    0.49 |      3.7231 |           - |           - |               316 B |
              MultiParam | 76.769 us | 0.8170 us | 0.7243 us | 34.66 |    0.41 |      4.6387 |           - |           - |              2675 B |
 MultiParamEnumerableKey | 65.731 us | 1.1941 us | 1.1728 us | 29.62 |    0.71 |      5.4932 |           - |           - |               324 B |


KeyCount = 1000
                  Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------------------ |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                Baseline |   233.8 us |  1.150 us |  1.076 us |  1.00 |    0.00 |     15.1367 |           - |           - |             32042 B |
               SingleKey | 5,138.0 us | 96.903 us | 85.902 us | 21.99 |    0.40 |    281.2500 |           - |           - |            256044 B |
           EnumerableKey | 1,217.3 us | 13.737 us | 12.177 us |  5.21 |    0.05 |    136.7188 |     46.8750 |           - |               320 B |
              MultiParam | 7,421.3 us | 75.340 us | 70.473 us | 31.75 |    0.36 |    460.9375 |           - |           - |            264111 B |
 MultiParamEnumerableKey | 1,422.1 us | 14.840 us | 13.881 us |  6.08 |    0.07 |    144.5313 |     72.2656 |           - |               328 B |
*/