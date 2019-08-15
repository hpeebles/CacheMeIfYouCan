using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Benchmarks.DuplicateTaskCatcherMulti")]
namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DuplicateTaskCatcherMulti<TK, TV> : IDuplicateTaskCatcherMulti<TK, TV>
    {
        private readonly Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> _func;
        private readonly IEqualityComparer<TK> _comparer;
        private readonly ConcurrentDictionary<TK, Task<ResultsMulti>> _tasks;
        private readonly ArrayPool<TK> _arrayPool;
        private const int ArrayPoolCutOffSize = 100;

        public DuplicateTaskCatcherMulti(
            Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func,
            IEqualityComparer<TK> comparer)
        {
            _func = func;
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), Messages.NoKeyComparerDefined<TK>());
            _tasks = new ConcurrentDictionary<TK, Task<ResultsMulti>>(comparer);
            _arrayPool = ArrayPool<TK>.Shared;
        }

        public async Task<IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>> ExecuteAsync(
            ICollection<TK> keys,
            CancellationToken token = default)
        {
            var tcs = new TaskCompletionSource<ResultsMulti>();
            var alreadyPending = new List<KeyValuePair<TK, Task<ResultsMulti>>>();

            var usePooledArray = keys.Count >= ArrayPoolCutOffSize;
            
            // In most cases the vast majority of requests will not be duplicates
            // so initialize this array with enough capacity to fit all keys
            var toFetch = usePooledArray
                ? _arrayPool.Rent(keys.Count)
                : new TK[keys.Count];

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
                    var values = await _func(new ArraySegment<TK>(toFetch, 0, toFetchCount), token);

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
                    if (!waitForPendingTask.IsCompleted)
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
                if (usePooledArray)
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