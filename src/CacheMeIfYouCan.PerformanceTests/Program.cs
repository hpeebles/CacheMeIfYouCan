using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
#if NETCOREAPP2_1
using BenchmarkDotNet.Diagnostics.Windows;
#endif
using BenchmarkDotNet.Running;

namespace CacheMeIfYouCan.PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner
                .Run<SingleVsMulti<Guid>>(ManualConfig
                .Create(DefaultConfig.Instance)
#if NETCOREAPP2_1
                .With(MemoryDiagnoser.Default)
                .With(new EtwProfiler()));
#elif NET45
                .With(MemoryDiagnoser.Default));
#endif
        }
    }
}