using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeysConfiguration<TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<IReadOnlyCollection<TKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public BatchBehaviour BatchBehaviour { get; set; }
    }
}