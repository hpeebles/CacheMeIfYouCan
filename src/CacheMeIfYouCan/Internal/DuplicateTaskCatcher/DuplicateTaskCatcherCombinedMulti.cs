using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DuplicateTaskCatcherCombinedMulti<TK1, TK2, TV>
    {
        private readonly Func<TK1, ICollection<TK2>, Task<IDictionary<TK2, TV>>> _func;
        private readonly IEqualityComparer<TK2> _innerKeyComparer;
        private readonly ConcurrentDictionary<(TK1, TK2), Task<ResultsMulti>> _tasks;
        private readonly ArrayPool<TK2> _arrayPool;
        private const int ArrayPoolCutOffSize = 100;

        public DuplicateTaskCatcherCombinedMulti(
            Func<TK1, ICollection<TK2>, Task<IDictionary<TK2, TV>>> func,
            IEqualityComparer<TK1> outerComparer,
            IEqualityComparer<TK2> innerComparer)
        {
            _func = func;
            _innerKeyComparer = innerComparer;
            
            var combinedComparer = new ValueTupleComparer<TK1, TK2>(outerComparer, innerComparer);
            
            _tasks = new ConcurrentDictionary<(TK1, TK2), Task<ResultsMulti>>(combinedComparer);
            _arrayPool = ArrayPool<TK2>.Shared;
        }

        public async Task<IDictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>> ExecuteAsync(
            TK1 outerKey,
            ICollection<TK2> innerKeys)
        {
            var tcs = new TaskCompletionSource<ResultsMulti>();
            var alreadyPending = new List<KeyValuePair<TK2, Task<ResultsMulti>>>();

            var usePooledArray = innerKeys.Count >= ArrayPoolCutOffSize;
            
            // In most cases the vast majority of requests will not be duplicates
            // so initialize this array with enough capacity to fit all keys
            var toFetch = usePooledArray
                ? _arrayPool.Rent(innerKeys.Count)
                : new TK2[innerKeys.Count];

            var toFetchCount = 0;
            foreach (var key in innerKeys)
            {
                var task = _tasks.GetOrAdd((outerKey, key), k => tcs.Task);

                if (task == tcs.Task)
                    toFetch[toFetchCount++] = key;
                else
                    alreadyPending.Add(new KeyValuePair<TK2, Task<ResultsMulti>>(key, task));
            }

            var waitForPendingTask = alreadyPending.Any()
                ? Task.WhenAll(alreadyPending.Select(kv => kv.Value).Distinct())
                : null;

            var results = new Dictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>(
                innerKeys.Count,
                _innerKeyComparer);
            
            try
            {
                if (toFetch.Any())
                {
                    var values = await _func(outerKey, new ArraySegment<TK2>(toFetch, 0, toFetchCount));

                    var resultsMulti = new ResultsMulti(outerKey, values);

                    tcs.SetResult(resultsMulti);

                    if (values != null)
                    {
                        foreach (var kv in values)
                        {
                            results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK2, TV>(
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
                            results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK2, TV>(
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
                
                foreach (var key in innerKeys)
                    _tasks.TryRemove((outerKey, key), out _);
            }
        }

        private class ResultsMulti
        {
            public ResultsMulti(TK1 outerKey, IDictionary<TK2, TV> results)
            {
                OuterKey = outerKey;
                Results = results ?? new Dictionary<TK2, TV>();
                StopwatchTimestampCompleted = Stopwatch.GetTimestamp();
            }
            
            public TK1 OuterKey { get; }
            public IDictionary<TK2, TV> Results { get; }
            public long StopwatchTimestampCompleted { get; }
        }
    }
}