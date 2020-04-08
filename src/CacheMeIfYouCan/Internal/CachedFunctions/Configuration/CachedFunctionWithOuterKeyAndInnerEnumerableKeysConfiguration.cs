using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Events.CachedFunction.OuterKeyAndInnerEnumerableKeys;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal abstract class CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationBase<TOuterKey, TInnerKey, TValue>
    {
        public TimeSpan? TimeToLive { get; set; }
        public ILocalCache<TOuterKey, TInnerKey, TValue> LocalCache { get; set; }
        public IDistributedCache<TOuterKey, TInnerKey, TValue> DistributedCache { get; set; }
        public IEqualityComparer<TInnerKey> KeyComparer { get; set; }
        public bool DisableCaching { get; set; }
        public (bool IsSet, TValue Value) FillMissingKeysConstantValue { get; set; }
        public Func<TOuterKey, TInnerKey, TValue> FillMissingKeysValueFactory { get; set; }
        public Func<TOuterKey, bool> SkipLocalCacheGetOuterPredicate { get; set; }
        public Func<TOuterKey, TInnerKey, bool> SkipLocalCacheGetInnerPredicate { get; set; }
        public Func<TOuterKey, bool> SkipLocalCacheSetOuterPredicate { get; set; }
        public Func<TOuterKey, TInnerKey, TValue, bool> SkipLocalCacheSetInnerPredicate { get; set; }
        public Func<TOuterKey, bool> SkipDistributedCacheGetOuterPredicate { get; set; }
        public Func<TOuterKey, TInnerKey, bool> SkipDistributedCacheGetInnerPredicate { get; set; }
        public Func<TOuterKey, bool> SkipDistributedCacheSetOuterPredicate { get; set; }
        public Func<TOuterKey, TInnerKey, TValue, bool> SkipDistributedCacheSetInnerPredicate { get; set; }
        public int MaxBatchSize { get; set; } = Int32.MaxValue;
        public BatchBehaviour BatchBehaviour { get; set; }
    }

    internal sealed class CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TParams, TOuterKey, TInnerKey, TValue>
        : CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationBase<TOuterKey, TInnerKey, TValue>
    {
        public Func<TParams, IReadOnlyCollection<TInnerKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public Func<TParams, bool> SkipCacheGetOuterPredicate { get; set; }
        public Func<TParams, TInnerKey, bool> SkipCacheGetInnerPredicate { get; set; }
        public Func<TParams, bool> SkipCacheSetOuterPredicate { get; set; }
        public Func<TParams, TInnerKey, TValue, bool> SkipCacheSetInnerPredicate { get; set; }
        public Action<SuccessfulRequestEvent<TParams, TOuterKey, TInnerKey, TValue>> OnSuccessAction { get; set; }
        public Action<ExceptionEvent<TParams, TOuterKey, TInnerKey>> OnExceptionAction { get; set; }
    }
}