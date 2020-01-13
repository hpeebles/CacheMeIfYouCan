using System;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TKey, TValue> Build<TKey, TValue>(
            CachedFunctionConfigurationBase<TKey, TValue> config,
            out Func<TKey, bool> additionalSkipCacheGetPredicate,
            out Func<TKey, TValue, bool> additionalSkipCacheSetPredicate)
        {
            additionalSkipCacheGetPredicate = null;
            additionalSkipCacheSetPredicate = null;
            
            if (config.DisableCaching)
                return NullCache<TKey, TValue>.Instance;
            
            if (config.LocalCache is null)
            {
                if (config.DistributedCache is null)
                    return NullCache<TKey, TValue>.Instance;

                additionalSkipCacheGetPredicate = config.SkipDistributedCacheGetPredicate;
                additionalSkipCacheSetPredicate = config.SkipDistributedCacheSetPredicate;
                
                return new DistributedCacheAdapter<TKey, TValue>(config.DistributedCache);
            }

            if (config.DistributedCache is null)
            {
                additionalSkipCacheGetPredicate = config.SkipLocalCacheGetPredicate;
                additionalSkipCacheSetPredicate = config.SkipLocalCacheSetPredicate;

                return new LocalCacheAdapter<TKey, TValue>(config.LocalCache);
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
            CachedFunctionWithOuterKeyAndInnerEnumerableKeysConfiguration<TOuterKey, TInnerKey, TValue> config,
            out Func<TOuterKey, bool> additionalSkipCacheGetPredicateOuterKeyOnly,
            out Func<TOuterKey, TInnerKey, bool> additionalSkipCacheGetPredicate,
            out Func<TOuterKey, bool> additionalSkipCacheSetPredicateOuterKeyOnly,
            out Func<TOuterKey, TInnerKey, TValue, bool> additionalSkipCacheSetPredicate)
        {
            additionalSkipCacheGetPredicateOuterKeyOnly = null;
            additionalSkipCacheGetPredicate = null;
            additionalSkipCacheSetPredicateOuterKeyOnly = null;
            additionalSkipCacheSetPredicate = null;
            
            if (config.DisableCaching)
                return NullCache<TOuterKey, TInnerKey, TValue>.Instance;
            
            if (config.LocalCache is null)
            {
                if (config.DistributedCache is null)
                    return NullCache<TOuterKey, TInnerKey, TValue>.Instance;

                additionalSkipCacheGetPredicateOuterKeyOnly = config.SkipDistributedCacheGetPredicateOuterKeyOnly;
                additionalSkipCacheGetPredicate = config.SkipDistributedCacheGetPredicate;
                additionalSkipCacheSetPredicateOuterKeyOnly = config.SkipDistributedCacheSetPredicateOuterKeyOnly;
                additionalSkipCacheSetPredicate = config.SkipDistributedCacheSetPredicate;
                
                return new DistributedCacheAdapter<TOuterKey, TInnerKey, TValue>(config.DistributedCache);
            }

            if (config.DistributedCache is null)
            {
                additionalSkipCacheGetPredicateOuterKeyOnly = config.SkipLocalCacheGetPredicateOuterKeyOnly;
                additionalSkipCacheGetPredicate = config.SkipLocalCacheGetPredicate;
                additionalSkipCacheSetPredicateOuterKeyOnly = config.SkipLocalCacheSetPredicateOuterKeyOnly;
                additionalSkipCacheSetPredicate = config.SkipLocalCacheSetPredicate;

                return new LocalCacheAdapter<TOuterKey, TInnerKey, TValue>(config.LocalCache);
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