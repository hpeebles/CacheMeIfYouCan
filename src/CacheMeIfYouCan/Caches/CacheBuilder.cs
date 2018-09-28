using System;
using CacheMeIfYouCan.Internal;

namespace CacheMeIfYouCan.Caches
{
    internal static class CacheBuilder
    {
        public static ICache<TK, TV> Build<TK, TV>(
            ILocalCacheFactory<TK, TV> localCacheFactory,
            ICacheFactory<TK, TV> remoteCacheFactory,
            CacheFactoryConfig<TK, TV> config,
            out bool requiresStringKeys)
        {
            if (localCacheFactory == null && remoteCacheFactory == null)
            {
                requiresStringKeys = true;
                return Default<TK, TV>();
            }

            var localCache = localCacheFactory?.Build();

            Action<Key<TK>> removeFromLocalCacheAction = null;
            if (localCache != null)
                removeFromLocalCacheAction = localCache.Remove;

            var remoteCache = remoteCacheFactory?.Build(config, removeFromLocalCacheAction);

            if (localCache != null)
            {
                requiresStringKeys = localCacheFactory.RequiresStringKeys;

                if (remoteCache != null)
                    return new TwoTierCache<TK, TV>(localCache, remoteCache);

                return new LocalCacheWrapper<TK, TV>(localCache);
            }

            if (remoteCache == null)
                throw new Exception("Cache factory returned null");

            requiresStringKeys = remoteCacheFactory.RequiresStringKeys;
            return remoteCache;
        }

        private static ICache<TK, TV> Default<TK, TV>()
        {
            var memoryCache = new MemoryCache<TK, TV>();
            
            return new LocalCacheWrapper<TK, TV>(memoryCache);
        }
    }
}