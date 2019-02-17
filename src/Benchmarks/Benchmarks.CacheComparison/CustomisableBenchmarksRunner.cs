using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CacheMeIfYouCan;
using CacheMeIfYouCan.Caches;

namespace Benchmarks.CacheComparison
{
    public class CustomisableBenchmarksRunner
    {
        private readonly ILocalCache<int, int> _memoryCache;
        private readonly ILocalCache<int, int> _dictionary;
        private readonly ILocalCache<int, int> _rollingTimeToLiveDictionary;
        private readonly Key<int>[] _keys;
        private const bool Multi = false;
        private const int KeysCount = 1000;
        private const int ParallelRequestsCount = 100;
        
        public CustomisableBenchmarksRunner()
        {
            _memoryCache = new MemoryCache<int, int>("1");
            _dictionary = new DictionaryCache<int, int>("2");
            _rollingTimeToLiveDictionary = new RollingTimeToLiveDictionaryCache<int, int>("3", TimeSpan.FromMinutes(1));

            _keys = Enumerable
                .Range(0, KeysCount)
                .Select(k => new Key<int>(k, x => x.ToString()))
                .ToArray();
        }
        
        public static void Run()
        {
#if DEBUG
            var runner = new CustomisableBenchmarksRunner();

            runner.MemoryCache();
            runner.Dictionary();
            runner.RollingTimeToLiveDictionary();
#else
            BenchmarkRunner.Run<CustomisableBenchmarksRunner>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(MemoryDiagnoser.Default)
                .With(Job.Default.With(new GcMode { Force = false })));
#endif
        }
        
        [Benchmark]
        public void MemoryCache()
        {
            Parallelize(GetAction(_memoryCache));
        }

        [Benchmark]
        public void Dictionary()
        {
            Parallelize(GetAction(_dictionary));
        }
        
        [Benchmark]
        public void RollingTimeToLiveDictionary()
        {
            Parallelize(GetAction(_rollingTimeToLiveDictionary));
        }
        
        private void Parallelize(Action action)
        {
            var tasks = Enumerable
                .Range(0, ParallelRequestsCount)
                .Select(_ => Task.Run(() => action))
                .ToArray();

            Task.WaitAll(tasks);
        }

        private Action GetAction(ILocalCache<int, int> cache)
        {
            if (Multi)
                return () => RunAsMulti(cache);

            return () => RunAsSingle(cache);
        }

        private void RunAsSingle(ILocalCache<int, int> cache)
        {
            foreach (var key in _keys)
                cache.Get(key);
            
            foreach (var key in _keys)
                cache.Set(key, key, TimeSpan.FromMinutes(1));
            
            foreach (var key in _keys)
                cache.Get(key);
        }

        private void RunAsMulti(ILocalCache<int, int> cache)
        {
            cache.Get(_keys);
            cache.Set(_keys.Select(k => new KeyValuePair<Key<int>, int>(k, k)).ToArray(), TimeSpan.FromMinutes(1));
            cache.Get(_keys);
        }
    }
}