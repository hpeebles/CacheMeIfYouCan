using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TKey, TValue> Build<TKey, TValue>(CachedFunctionConfigurationBase<TKey, TValue> config)
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
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfigurationBase<TOuterKey, TInnerKey, TValue> config)
        {
            if (config.DisableCaching)
                return NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            
            if (config.LocalCache is null)
            {
                if (config.DistributedCache is null)
                    return NullCache<TOuterKey, TInnerKey, TValue>.Instance;

                return new DistributedCacheAdapter<TOuterKey, TInnerKey, TValue>(
                    config.DistributedCache,
                    config.SkipDistributedCacheGetOuterPredicate,
                    config.SkipDistributedCacheGetInnerPredicate,
                    config.SkipDistributedCacheSetOuterPredicate,
                    config.SkipDistributedCacheSetInnerPredicate);
            }

            if (config.DistributedCache is null)
            {
                return new LocalCacheAdapter<TOuterKey, TInnerKey, TValue>(
                    config.LocalCache,
                    config.SkipLocalCacheGetOuterPredicate,
                    config.SkipLocalCacheGetInnerPredicate,
                    config.SkipLocalCacheSetOuterPredicate,
                    config.SkipLocalCacheSetInnerPredicate);
            }

            return new TwoTierCache<TOuterKey, TInnerKey, TValue>(
                config.LocalCache,
                config.DistributedCache,
                config.KeyComparer,
                config.SkipLocalCacheGetOuterPredicate,
                config.SkipLocalCacheGetInnerPredicate,
                config.SkipLocalCacheSetOuterPredicate,
                config.SkipLocalCacheSetInnerPredicate,
                config.SkipDistributedCacheGetOuterPredicate,
                config.SkipDistributedCacheGetInnerPredicate,
                config.SkipDistributedCacheSetOuterPredicate,
                config.SkipDistributedCacheSetInnerPredicate);
        }
    }
}