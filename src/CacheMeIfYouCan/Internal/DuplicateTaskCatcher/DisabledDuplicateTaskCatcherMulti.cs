using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DisabledDuplicateTaskCatcherMulti<TK, TV> : IDuplicateTaskCatcherMulti<TK, TV>
    {
        private readonly Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> _func;
        private readonly IEqualityComparer<TK> _keyComparer;

        public DisabledDuplicateTaskCatcherMulti(
            Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> func,
            IEqualityComparer<TK> keyComparer)
        {
            _func = func;
            _keyComparer = keyComparer;
        }

        public async Task<IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>> ExecuteAsync(
            ICollection<TK> keys,
            CancellationToken token = default)
        {
            var results = new Dictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>(keys.Count, _keyComparer);
            
            var values = await _func(keys, token);
            
            if (values != null)
            {
                var stopwatchTimestamp = Stopwatch.GetTimestamp();
                
                foreach (var kv in values)
                {
                    results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK, TV>(
                        kv.Key,
                        kv.Value,
                        false,
                        stopwatchTimestamp);
                }
            }

            return results;
        }
    }
}