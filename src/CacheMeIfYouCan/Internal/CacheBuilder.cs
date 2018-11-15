using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TK, TV> Build<TK, TV>(
            string cacheName,
            ILocalCacheFactory<TK, TV> localCacheFactory,
            ICacheFactory<TK, TV> remoteCacheFactory,
            CacheFactoryConfig<TK, TV> config,
            Action<CacheGetResult<TK, TV>> onCacheGet,
            Action<CacheSetResult<TK, TV>> onCacheSet,
            out IEqualityComparer<Key<TK>> keyComparer)
        {
            keyComparer = new StringKeyComparer<TK>();

            if (localCacheFactory == null && remoteCacheFactory == null)
                localCacheFactory = GetDefaultLocalCacheFactory<TK, TV>();

            var localCache = localCacheFactory?
                .Configure(c => c
                    .OnGetResult(onCacheGet)
                    .OnSetResult(onCacheSet))
                .Build(cacheName);

            if (localCache is ICachedItemCounter localItemCounter)
                CachedItemCounterContainer.Register(localItemCounter);
            
            var remoteCache = remoteCacheFactory?
                .Configure(c => c
                    .OnGetResult(onCacheGet)
                    .OnSetResult(onCacheSet))
                .Build(config);

            if (remoteCache is ICachedItemCounter remoteItemCounter)
                CachedItemCounterContainer.Register(remoteItemCounter);

            if (localCache != null)
            {
                if (!localCacheFactory.RequiresStringKeys)
                    keyComparer = new GenericKeyComparer<TK>();
                
                if (remoteCache != null)
                    return new TwoTierCache<TK, TV>(localCache, remoteCache, keyComparer);

                return new LocalCacheAdaptor<TK, TV>(localCache);
            }

            if (remoteCache == null)
                throw new Exception("Cache factory returned null");

            if (!remoteCacheFactory.RequiresStringKeys)
                keyComparer = new GenericKeyComparer<TK>();
            
            return remoteCache;
        }

        private static ILocalCacheFactory<TK, TV> GetDefaultLocalCacheFactory<TK, TV>()
        {
            return new LocalCacheFactoryWrapper<TK, TV>(new MemoryCacheFactory());
        }
    }
}
