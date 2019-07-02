using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal.DuplicateTaskCatcher
{
    internal class DisabledDuplicateTaskCatcherCombinedMulti<TK1, TK2, TV> : IDuplicateTaskCatcherCombinedMulti<TK1, TK2, TV>
    {
        private readonly Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> _func;
        private readonly IEqualityComparer<TK2> _keyComparer;
        private static readonly IDictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>> EmptyDictionary =
            new ReadOnlyDictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>(new Dictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>());

        public DisabledDuplicateTaskCatcherCombinedMulti(
            Func<TK1, ICollection<TK2>, CancellationToken, Task<IDictionary<TK2, TV>>> func,
            IEqualityComparer<TK2> keyComparer)
        {
            _func = func;
            _keyComparer = keyComparer;
        }
        
        public async Task<IDictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>> ExecuteAsync(
            TK1 outerKey,
            ICollection<TK2> innerKeys,
            CancellationToken token)
        {
            var values = await _func(outerKey, innerKeys, token);

            if (values == null || values.Count == 0)
                return EmptyDictionary;
                
            var results = new Dictionary<TK2, DuplicateTaskCatcherMultiResult<TK2, TV>>(innerKeys.Count, _keyComparer);

            var stopwatchTimestamp = Stopwatch.GetTimestamp();

            foreach (var kv in values)
            {
                results[kv.Key] = new DuplicateTaskCatcherMultiResult<TK2, TV>(
                    kv.Key,
                    kv.Value,
                    false,
                    stopwatchTimestamp);
            }

            return results;
        }
    }
}