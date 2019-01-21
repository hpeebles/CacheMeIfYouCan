using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;

namespace Benchmarks.DuplicateTaskCatcherMulti
{
    public class CustomisableBenchmarksRunner
    {
        private readonly DuplicateTaskCatcherMulti<string, string> _current;
        private readonly DuplicateTaskCatcherMulti_1<string, string> _variant1;
        private readonly DuplicateTaskCatcherMulti_2<string, string> _variant2;
        private readonly DuplicateTaskCatcherMulti_3<string, string> _variant3;
        private readonly string[] _keys;
        private readonly TimeSpan _delay = TimeSpan.FromTicks(1);
        private const int KeysCount = 1000;
        private const int ParallelRequestsCount = 10;
        
        public CustomisableBenchmarksRunner()
        {
            _current = new DuplicateTaskCatcherMulti<string, string>(
                Func,
                StringComparer.Ordinal);
            
            _variant1 = new DuplicateTaskCatcherMulti_1<string, string>(
                Func,
                StringComparer.Ordinal);
            
            _variant2 = new DuplicateTaskCatcherMulti_2<string, string>(
                Func,
                StringComparer.Ordinal);
            
            _variant3 = new DuplicateTaskCatcherMulti_3<string, string>(
                Func,
                StringComparer.Ordinal);

            _keys = Enumerable
                .Range(0, KeysCount)
                .Select(_ => Guid.NewGuid().ToString())
                .ToArray();

            async Task<IDictionary<string, string>> Func(ICollection<string> keys)
            {
                await Task.Delay(_delay);
                
                return keys.ToDictionary(k => k, k => k);
            }
        }
        
        public static void Run()
        {
#if DEBUG
            var runner = new CustomisableBenchmarksRunner();

            runner.Current();
            runner.Variant1();
            runner.Variant2();
            runner.Variant3();
#else
            BenchmarkRunner.Run<CustomisableBenchmarksRunner>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default)
                .With(Job.Default.With(new GcMode { Force = false })));
#endif
        }
        
        [Benchmark(Baseline = true)]
        public void Current()
        {
            Parallelize(() => _current.ExecuteAsync(_keys));
        }

        [Benchmark]
        public void Variant1()
        {
            Parallelize(() => _variant1.ExecuteAsync(_keys));
        }

        [Benchmark]
        public void Variant2()
        {
            Parallelize(() => _variant2.ExecuteAsync(_keys));
        }
        
        [Benchmark]
        public void Variant3()
        {
            Parallelize(() => _variant3.ExecuteAsync(_keys));
        }

        private void Parallelize(Func<Task> func)
        {
            var tasks = Enumerable
                .Range(0, ParallelRequestsCount)
                .Select(_ => func())
                .ToArray();

            Task.WaitAll(tasks);
        }
    }
}