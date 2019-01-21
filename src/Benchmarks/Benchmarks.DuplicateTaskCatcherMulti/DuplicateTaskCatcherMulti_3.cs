using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal.DuplicateTaskCatcher;

namespace Benchmarks.DuplicateTaskCatcherMulti
{
    // Always takes array from pool
    internal class DuplicateTaskCatcherMulti_3<TK, TV>
    {
        private readonly Func<ICollection<TK>, Task<IDictionary<TK, TV>>> _func;
        private readonly IEqualityComparer<TK> _comparer;
        private readonly ConcurrentDictionary<TK, Task<ResultsMulti>> _tasks;
        private readonly ArrayPool<TK> _arrayPool;

        public DuplicateTaskCatcherMulti_3(Func<ICollection<TK>, Task<IDictionary<TK, TV>>> func, IEqualityComparer<TK> comparer)
        {
            _func = func;
            _comparer = comparer;
            _tasks = new ConcurrentDictionary<TK, Task<ResultsMulti>>(comparer);
            _arrayPool = ArrayPool<TK>.Shared;
        }

        public async Task<IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>> ExecuteAsync(ICollection<TK> keys)
        {
            var tcs = new TaskCompletionSource<ResultsMulti>();
            var alreadyPending = new List<KeyValuePair<TK, Task<ResultsMulti>>>();
            
            // In most cases the vast majority of requests will not be duplicates
            // so initialize this array with enough capacity to fit all keys
            var toFetch = _arrayPool.Rent(keys.Count);

            var toFetchCount = 0;
            foreach (var key in keys)
            {
                var task = _tasks.GetOrAdd(key, k => tcs.Task);

                if (task == tcs.Task)
                    toFetch[toFetchCount++] = key;
                else
                    alreadyPending.Add(new KeyValuePair<TK, Task<ResultsMulti>>(key, task));
            }

            var waitForPendingTask = alreadyPending.Any()
                ? Task.WhenAll(alreadyPending.Select(kv => kv.Value).Distinct())
                : null;

            var results = new Dictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>(keys.Count, _comparer);
            try
            {
                if (toFetch.Any())
                {
                    var values = await _func(new ArraySegment<TK>(toFetch, 0, toFetchCount));

                    var resultsMulti = new ResultsMulti(values);

                    tcs.SetResult(resultsMulti);

                    if (values != null)
                    {
                        foreach (var kv in values)
                        {
                            results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK, TV>(
                                kv.Key,
                                kv.Value,
                                false,
                                resultsMulti.StopwatchTimestampCompleted);
                        }
                    }
                }
                else
                {
                    tcs.SetCanceled();
                }

                if (waitForPendingTask != null)
                {
                    await waitForPendingTask;

                    foreach (var kv in alreadyPending)
                    {
                        if (kv.Value.Result.Results.TryGetValue(kv.Key, out var value))
                        {
                            results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK, TV>(
                                kv.Key,
                                value,
                                true,
                                kv.Value.Result.StopwatchTimestampCompleted);
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);

                throw;
            }
            finally
            {
                _arrayPool.Return(toFetch);
                
                foreach (var key in keys)
                    _tasks.TryRemove(key, out _);
            }
        }

        private class ResultsMulti
        {
            public ResultsMulti(IDictionary<TK, TV> results)
            {
                Results = results ?? new Dictionary<TK, TV>();
                StopwatchTimestampCompleted = Stopwatch.GetTimestamp();
            }
            
            public IDictionary<TK, TV> Results { get; }
            public long StopwatchTimestampCompleted { get; }
        }
    }
}