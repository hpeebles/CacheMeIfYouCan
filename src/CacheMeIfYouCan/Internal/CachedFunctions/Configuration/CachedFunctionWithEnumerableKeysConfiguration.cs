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
        public (bool IsSet, TValue Value) FillMissingKeysConstantValue { get; set; }
        public Func<TKey, TValue> FillMissingKeysValueFactory { get; set; }
        public Func<TKey, TValue, bool> FilterResponsePredicate { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public Func<TParams, ReadOnlyMemory<TKey>, int> MaxBatchSizeFactory { get; set; }
        public BatchBehaviour BatchBehaviour { get; set; }
        public Action<SuccessfulRequestEvent<TParams, TKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent<TParams, TKey>> OnExceptionAction { get; set; }
    }
}