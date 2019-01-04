using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICacheInternal<TK, TV> Build<TK, TV>(
            string cacheName,
            ILocalCacheFactory<TK, TV> localCacheFactory,
            IDistributedCacheFactory<TK, TV> distributedCacheFactory,
            DistributedCacheConfig<TK, TV> config,
            Action<CacheGetResult<TK, TV>> onCacheGet,
            Action<CacheSetResult<TK, TV>> onCacheSet,
            Action<CacheException<TK>> onCacheException,
            IEqualityComparer<Key<TK>> keyComparer)
        {
            if (localCacheFactory == null && distributedCacheFactory == null)
                localCacheFactory = GetDefaultLocalCacheFactory<TK, TV>();

            ILocalCache<TK, TV> localCache;
            if (localCacheFactory is NullLocalCacheFactory<TK, TV>)
            {
                localCache = null;
            }
            else
            {
                localCache = localCacheFactory
                    ?.OnGetResult(onCacheGet)
                    .OnSetResult(onCacheSet)
                    .OnException(onCacheException)
                    .Build(cacheName);
            }

            if (localCache is ICachedItemCounter localItemCounter)
                CachedItemCounterContainer.Register(localItemCounter);
            
            IDistributedCache<TK, TV> distributedCache;
            if (distributedCacheFactory is NullDistributedCacheFactory<TK, TV>)
            {
                distributedCache = null;
            }
            else
            {
                distributedCache = distributedCacheFactory
                    ?.OnGetResult(onCacheGet)
                    .OnSetResult(onCacheSet)
                    .OnException(onCacheException)
                    .WithKeyspacePrefix(config.KeyspacePrefix)
                    .Build(config);
            }

            if (distributedCache is ICachedItemCounter distributedItemCounter)
                CachedItemCounterContainer.Register(distributedItemCounter);

            if (localCache != null)
            {
                if (distributedCache != null)
                    return new TwoTierCache<TK, TV>(localCache, distributedCache, keyComparer);

                return new LocalCacheToCacheInternalAdapter<TK, TV>(localCache);
            }

            if (distributedCache == null)
                throw new Exception("Cache factory returned null");

            return new DistributedCacheToCacheInternalAdapter<TK, TV>(distributedCache);
        }

        private static ILocalCacheFactory<TK, TV> GetDefaultLocalCacheFactory<TK, TV>()
        {
            return new LocalCacheFactoryToGenericAdapter<TK, TV>(new MemoryCacheFactory());
        }
    }
}
