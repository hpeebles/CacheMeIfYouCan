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
            Action<CacheException<TK>> onCacheError,
            out IEqualityComparer<Key<TK>> keyComparer)
        {
            keyComparer = new StringKeyComparer<TK>();

            if (localCacheFactory == null && distributedCacheFactory == null)
                localCacheFactory = GetDefaultLocalCacheFactory<TK, TV>();

            var localCache = localCacheFactory
                ?.OnGetResult(onCacheGet)
                .OnSetResult(onCacheSet)
                .OnError(onCacheError)
                .Build(cacheName);

            if (localCache is ICachedItemCounter localItemCounter)
                CachedItemCounterContainer.Register(localItemCounter);
            
            var distributedCache = distributedCacheFactory
                ?.OnGetResult(onCacheGet)
                .OnSetResult(onCacheSet)
                .OnError(onCacheError)
                .WithKeyspacePrefix(config.KeyspacePrefix)
                .Build(config);

            if (distributedCache is ICachedItemCounter distributedItemCounter)
                CachedItemCounterContainer.Register(distributedItemCounter);

            if (localCache != null)
            {
                if (!localCacheFactory.RequiresStringKeys)
                    keyComparer = new GenericKeyComparer<TK>();
                
                if (distributedCache != null)
                    return new TwoTierCache<TK, TV>(localCache, distributedCache, keyComparer);

                return new LocalCacheAdaptorInternal<TK, TV>(localCache);
            }

            if (distributedCache == null)
                throw new Exception("Cache factory returned null");

            if (!distributedCacheFactory.RequiresStringKeys)
                keyComparer = new GenericKeyComparer<TK>();
            
            return new DistributedCacheAdaptorInternal<TK, TV>(distributedCache);
        }

        private static ILocalCacheFactory<TK, TV> GetDefaultLocalCacheFactory<TK, TV>()
        {
            return new LocalCacheFactoryAdaptor<TK, TV>(new MemoryCacheFactory());
        }
    }
}
