using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;

namespace Benchmarks.DuplicateTaskCatcherMulti.Benchmarks
{
    public class NoDuplicates
    {
        private readonly DuplicateTaskCatcherMulti<string, string> _current;
        private readonly DuplicateTaskCatcherMulti_1<string, string> _alt1;
        private readonly string[] _keys;
        private readonly TimeSpan _delay = TimeSpan.FromMilliseconds(100);
        private const int KeysCount = 1000;
        
        public NoDuplicates()
        {
            _current = new DuplicateTaskCatcherMulti<string, string>(
                Func,
                StringComparer.Ordinal);
            
            _alt1 = new DuplicateTaskCatcherMulti_1<string, string>(
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
            var runner = new NoDuplicates();

            runner.Current();
            runner.Alt1();
#else
            BenchmarkRunner.Run<NoDuplicates>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default));
#endif
        }

        [Benchmark]
        public void Current()
        {
            _current.ExecuteAsync(_keys).Wait();
        }

        [Benchmark]
        public void Alt1()
        {
            _alt1.ExecuteAsync(_keys).Wait();
        }
    }
}