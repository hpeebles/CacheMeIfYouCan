using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TKey, TValue> Build<TKey, TValue>(
            ILocalCache<TKey, TValue> localCache,
            IDistributedCache<TKey, TValue> distributedCache,
            IEqualityComparer<TKey> keyComparer)
        {
            if (localCache is null)
            {
                if (distributedCache is null)
                    return NullCache<TKey, TValue>.Instance;

                return new DistributedCacheAdapter<TKey, TValue>(distributedCache);
            }

            if (distributedCache is null)
                return new LocalCacheAdapter<TKey, TValue>(localCache);
            
            return new TwoTierCache<TKey, TValue>(localCache, distributedCache, keyComparer);
        }
    }
}