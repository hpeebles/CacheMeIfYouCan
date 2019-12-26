using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TKey, TValue> Build<TKey, TValue>(
            CachedFunctionConfigurationBase<TKey, TValue> config)
        {
            if (config.DisableCaching)
                return NullCache<TKey, TValue>.Instance;
            
            if (config.LocalCache is null)
            {
                if (config.DistributedCache is null)
                    return NullCache<TKey, TValue>.Instance;

                return new DistributedCacheAdapter<TKey, TValue>(
                    config.DistributedCache,
                    config.SkipDistributedCacheGetPredicate,
                    config.SkipDistributedCacheSetPredicate);
            }

            if (config.DistributedCache is null)
            {
                return new LocalCacheAdapter<TKey, TValue>(
                    config.LocalCache,
                    config.SkipLocalCacheGetPredicate,
                    config.SkipLocalCacheSetPredicate);
            }

            return new TwoTierCache<TKey, TValue>(
                config.LocalCache,
                config.DistributedCache,
                config.KeyComparer,
                config.SkipLocalCacheGetPredicate,
                config.SkipLocalCacheSetPredicate,
                config.SkipDistributedCacheGetPredicate,
                config.SkipDistributedCacheSetPredicate);
        }
        
        public static ICache<TOuterKey, TInnerKey, TValue> Build<TOuterKey, TInnerKey, TValue>(
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue> config)
        {
            if (config.DisableCaching)
                return NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            
            if (config.LocalCache is null)
            {
                if (config.DistributedCache is null)
                    return NullCache<TOuterKey, TInnerKey, TValue>.Instance;

                return new DistributedCacheAdapter<TOuterKey, TInnerKey, TValue>(
                    config.DistributedCache,
                    config.SkipCacheGetPredicateOuterKeyOnly,
                    config.SkipDistributedCacheGetPredicate,
                    config.SkipCacheSetPredicateOuterKeyOnly,
                    config.SkipDistributedCacheSetPredicate);
            }

            if (config.DistributedCache is null)
            {
                return new LocalCacheAdapter<TOuterKey, TInnerKey, TValue>(
                    config.LocalCache,
                    config.SkipCacheGetPredicateOuterKeyOnly,
                    config.SkipLocalCacheGetPredicate,
                    config.SkipCacheSetPredicateOuterKeyOnly,
                    config.SkipLocalCacheSetPredicate);
            }

            return new TwoTierCache<TOuterKey, TInnerKey, TValue>(
                config.LocalCache,
                config.DistributedCache,
                config.KeyComparer,
                config.SkipLocalCacheGetPredicateOuterKeyOnly,
                config.SkipLocalCacheGetPredicate,
                config.SkipLocalCacheSetPredicateOuterKeyOnly,
                config.SkipLocalCacheSetPredicate,
                config.SkipDistributedCacheGetPredicateOuterKeyOnly,
                config.SkipDistributedCacheGetPredicate,
                config.SkipDistributedCacheSetPredicateOuterKeyOnly,
                config.SkipDistributedCacheSetPredicate);
        }
    }
}