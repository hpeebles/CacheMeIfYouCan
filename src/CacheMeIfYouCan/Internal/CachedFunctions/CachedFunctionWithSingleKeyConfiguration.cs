using System;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithSingleKeyConfiguration<TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<TKey, TimeSpan> TimeToLiveFactory { get; set; }
        public Func<TKey, bool> SkipCacheGetPredicate { get; set; }
        public Func<TKey, TValue, bool> SkipCacheSetPredicate { get; set; }
    }
}