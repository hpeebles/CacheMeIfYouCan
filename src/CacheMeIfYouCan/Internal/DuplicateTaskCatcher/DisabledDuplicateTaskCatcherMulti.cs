using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DisabledDuplicateTaskCatcherMulti<TK, TV> : IDuplicateTaskCatcherMulti<TK, TV>
    {
        private readonly Func<ICollection<TK>, CancellationToken, Task<IDictionary<TK, TV>>> _func;
        private readonly IEqualityComparer<TK> _keyComparer;
        private static readonly IDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>> EmptyDictionary =
            new ReadOnlyDictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>(new Dictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>());

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
            var values = await _func(keys, token);

            if (values == null || values.Count == 0)
                return EmptyDictionary;

            var results = new Dictionary<TK, DuplicateTaskCatcherMultiResult<TK, TV>>(keys.Count, _keyComparer);
   
            var stopwatchTimestamp = Stopwatch.GetTimestamp();
            
            foreach (var kv in values)
            {
                results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK, TV>(
                    kv.Key,
                    kv.Value,
                    false,
                    stopwatchTimestamp);
            }

            return results;
        }
    }
}