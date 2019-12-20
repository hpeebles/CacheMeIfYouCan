namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TKey, TValue> Build<TKey, TValue>(
            CachedFunctionConfiguration<TKey, TValue> config)
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
    }
}