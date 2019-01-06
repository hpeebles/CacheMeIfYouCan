using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal class DuplicateTaskCatcherSingle<TK, TV>
    {
        private readonly Func<TK, Task<TV>> _func;
        private readonly ConcurrentDictionary<TK, Task<ValueWithTimestamp<TV>>> _tasks;
        
        public DuplicateTaskCatcherSingle(Func<TK, Task<TV>> func, IEqualityComparer<TK> comparer)
        {
            _func = func;
            _tasks = new ConcurrentDictionary<TK, Task<ValueWithTimestamp<TV>>>(comparer);
        }

        public async Task<(ValueWithTimestamp<TV> value, bool duplicate)> ExecuteAsync(TK key)
        {
            var tcs = new TaskCompletionSource<ValueWithTimestamp<TV>>();

            var task = _tasks.GetOrAdd(key, tcs.Task);

            if (task != tcs.Task)
                return (await task, true);

            try
            {
                var result = await _func(key);

                var returnValue = new ValueWithTimestamp<TV>(result, Stopwatch.GetTimestamp());
                
                tcs.SetResult(returnValue);

                return (returnValue, false);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);

                throw;
            }
            finally
            {
                _tasks.TryRemove(key, out _);
            }
        }
    }
}