using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal abstract class CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationBase<TOuterKey, TInnerKey, TValue>
    {
        public Func<TOuterKey, IReadOnlyCollection<TInnerKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public ILocalCache<TOuterKey, TInnerKey, TValue> LocalCache { get; set; }
        public IDistributedCache<TOuterKey, TInnerKey, TValue> DistributedCache { get; set; }
        public IEqualityComparer<TInnerKey> KeyComparer { get; set; }
        public bool DisableCaching { get; set; }
        public (bool IsSet, TValue Value) FillMissingKeysConstantValue { get; set; }
        public Func<TOuterKey, TInnerKey, TValue> FillMissingKeysValueFactory { get; set; }
        public Func<TOuterKey, bool> SkipCacheGetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, bool> SkipCacheGetPredicate { get; set; }
        public Func<TOuterKey, bool> SkipCacheSetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, TValue, bool> SkipCacheSetPredicate { get; set; }
        public Func<TOuterKey, bool> SkipLocalCacheGetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, bool> SkipLocalCacheGetPredicate { get; set; }
        public Func<TOuterKey, bool> SkipLocalCacheSetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, TValue, bool> SkipLocalCacheSetPredicate { get; set; }
        public Func<TOuterKey, bool> SkipDistributedCacheGetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, bool> SkipDistributedCacheGetPredicate { get; set; }
        public Func<TOuterKey, bool> SkipDistributedCacheSetPredicateOuterKeyOnly { get; set; }
        public Func<TOuterKey, TInnerKey, TValue, bool> SkipDistributedCacheSetPredicate { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public BatchBehaviour BatchBehaviour { get; set; }
    }

    internal sealed class CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TParams, TOuterKey, TInnerKey, TValue>
        : CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationBase<TOuterKey, TInnerKey, TValue>
    {
        public Action<SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent<TParams, TOuterKey, TInnerKey>> OnExceptionAction { get; set; }
    }
}