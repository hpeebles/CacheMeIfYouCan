using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal.CachedFunctions.Configuration
{
    internal abstract class CachedFunctionConfigurationBase<TKey, TValue>
    {
        public ILocalCache<TKey, TValue> LocalCache { get; set; }
        public IDistributedCache<TKey, TValue> DistributedCache { get; set; }
        public IEqualityComparer<TKey> KeyComparer { get; set; }
        public bool DisableCaching { get; set; }
        public (bool IsSet, TValue Value) FillMissingKeysConstantValue { get; set; }
        public Func<TKey, TValue> FillMissingKeysValueFactory { get; set; }
        public Func<TKey, bool> SkipCacheGetPredicate { get; set; }
        public Func<TKey, TValue, bool> SkipCacheSetPredicate { get; set; }
        public Func<TKey, bool> SkipLocalCacheGetPredicate { get; set; }
        public Func<TKey, TValue, bool> SkipLocalCacheSetPredicate { get; set; }
        public Func<TKey, bool> SkipDistributedCacheGetPredicate { get; set; }
        public Func<TKey, TValue, bool> SkipDistributedCacheSetPredicate { get; set; }
    }
}