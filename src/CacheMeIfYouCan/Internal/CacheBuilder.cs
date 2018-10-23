using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheBuilder
    {
        public static ICache<TK, TV> Build<TK, TV>(
            FunctionInfo functionInfo,
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

            var localCache = localCacheFactory?.Build();

            Action<Key<TK>> removeFromLocalCacheAction = null;
            if (localCache != null)
                removeFromLocalCacheAction = localCache.Remove;

            var remoteCache = remoteCacheFactory?.Build(config, removeFromLocalCacheAction);

            if (onCacheGet != null || onCacheSet != null)
            {
                if (localCache != null)
                    localCache = new LocalCacheNotificationWrapper<TK, TV>(functionInfo, localCache, onCacheGet, onCacheSet);
                
                if (remoteCache != null)
                    remoteCache = new CacheNotificationWrapper<TK, TV>(functionInfo, remoteCache, onCacheGet, onCacheSet);
            }
            
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