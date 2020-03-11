using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal sealed class CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<IReadOnlyCollection<TKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public BatchBehaviour BatchBehaviour { get; set; }
        public Action<SuccessfulRequestEvent_MultiParam<TParams, TKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent_MultiParam<TParams, TKey>> OnExceptionAction { get; set; }
    }
}