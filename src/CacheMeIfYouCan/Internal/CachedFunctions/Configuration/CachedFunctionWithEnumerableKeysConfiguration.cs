using System;
using CacheMeIfYouCan.Events.CachedFunction.EnumerableKeys;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal sealed class CachedFunctionWithEnumerableKeysConfiguration<TParams, TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TParams, ReadOnlyMemory<TKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public Func<TParams, bool> SkipCacheGetOuterPredicate { get; set; }
        public Func<TParams, TKey, bool> SkipCacheGetInnerPredicate { get; set; }
        public Func<TParams, bool> SkipCacheSetOuterPredicate { get; set; }
        public Func<TParams, TKey, TValue, bool> SkipCacheSetInnerPredicate { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public BatchBehaviour BatchBehaviour { get; set; }
        public Action<SuccessfulRequestEvent<TParams, TKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent<TParams, TKey>> OnExceptionAction { get; set; }
    }
}