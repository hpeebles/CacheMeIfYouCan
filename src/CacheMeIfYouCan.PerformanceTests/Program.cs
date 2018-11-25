using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
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
                .With(MemoryDiagnoser.Default)
                .With(new EtwProfiler()));
        }
    }
}