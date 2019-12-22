using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.CachedFunctions
{
    internal sealed class CachedFunctionWithEnumerableKeysConfiguration<TKey, TValue> : CachedFunctionConfigurationBase<TKey, TValue>
    {
        public Func<IReadOnlyCollection<TKey>, TimeSpan> TimeToLiveFactory { get; set; }
        public Func<TKey, bool> SkipCacheGetPredicate { get; set; }
        public Func<TKey, TValue, bool> SkipCacheSetPredicate { get; set; }
    }
}