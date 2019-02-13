using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CacheMeIfYouCan.Caches;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Internal.DistributedCache;
using CacheMeIfYouCan.Internal.LocalCache;
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
            Action<CacheRemoveResult<TK>> onCacheRemove,
            Action<CacheException<TK>> onCacheException,
            IList<(IObservable<TK> keysToRemove, bool removeFromLocalOnly)> keyRemovalObservables,
            TimeSpan? localCacheTimeToLiveOverride)
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
                    .OnRemoveResult(onCacheRemove)
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
                    .OnRemoveResult(onCacheRemove)
                    .OnException(onCacheException)
                    .WithKeyspacePrefix(config.KeyspacePrefix)
                    .Build(config);
            }

            if (distributedCache is ICachedItemCounter distributedItemCounter)
                CachedItemCounterContainer.Register(distributedItemCounter);

            SetupKeyRemovalObservables(localCache, distributedCache, keyRemovalObservables, config.KeySerializer);
            
            if (localCache != null)
            {
                if (distributedCache != null)
                    return new TwoTierCache<TK, TV>(localCache, distributedCache, config.KeyComparer, localCacheTimeToLiveOverride);

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

        private static void SetupKeyRemovalObservables<TK, TV>(
            ILocalCache<TK, TV> localCache,
            IDistributedCache<TK, TV> distributedCache,
            IList<(IObservable<TK>, bool)> keysToRemoveObservables,
            Func<TK, string> keySerializer)
        {
            if (keysToRemoveObservables == null || !keysToRemoveObservables.Any())
                return;

            if (localCache != null)
            {
                keysToRemoveObservables
                    .Select(o => o.Item1)
                    .Merge()
                    .Select(k => localCache.Remove(new Key<TK>(k, keySerializer)))
                    .Retry()
                    .Subscribe();
            }

            if (distributedCache == null)
                return;

            var removeFromDistributed = keysToRemoveObservables
                .Where(o => !o.Item2)
                .Select(o => o.Item1)
                .ToArray();

            if (removeFromDistributed.Any())
            {
                removeFromDistributed
                    .Merge()
                    .SelectMany(k => distributedCache.Remove(new Key<TK>(k, keySerializer)))
                    .Retry()
                    .Subscribe();
            }
        }
    }
}
