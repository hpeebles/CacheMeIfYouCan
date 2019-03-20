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
            ILocalCacheFactory<TK, TV> localCacheFactory,
            IDistributedCacheFactory<TK, TV> distributedCacheFactory,
            IDistributedCacheConfig<TK, TV> config,
            Action<CacheGetResult<TK, TV>> onCacheGet,
            Action<CacheSetResult<TK, TV>> onCacheSet,
            Action<CacheRemoveResult<TK>> onCacheRemove,
            Action<CacheException<TK>> onCacheException,
            List<(IObservable<Key<TK>> keysToRemove, bool removeFromLocalOnly)> keyRemovalObservables,
            Func<TimeSpan> localCacheTimeToLiveOverride)
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
                    .Build(config);
            }

            if (localCache is WrappedLocalCacheWithOriginal<TK, TV> l)
            {
                localCache = l.Wrapped;
                var originalLocalCache = l.Original;

                if (originalLocalCache is ICachedItemCounter localItemCounter)
                    CachedItemCounterContainer.Register(localItemCounter);
            }
            
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

            if (distributedCache is WrappedDistributedCacheWithOriginal<TK, TV> d)
            {
                distributedCache = d.Wrapped;
                var originalDistributedCache = d.Original;

                if (originalDistributedCache is ICachedItemCounter distributedItemCounter)
                    CachedItemCounterContainer.Register(distributedItemCounter);

                if (originalDistributedCache is INotifyKeyChanges<TK> notifier && notifier.NotifyKeyChangesEnabled && localCache != null)
                    keyRemovalObservables.Add((notifier.KeyChanges, true));
            }

            SetupKeyRemovalObservables(localCache, distributedCache, keyRemovalObservables);
            
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
            IList<(IObservable<Key<TK>>, bool)> keysToRemoveObservables)
        {
            if (keysToRemoveObservables == null || !keysToRemoveObservables.Any())
                return;

            if (localCache != null)
            {
                keysToRemoveObservables
                    .Select(o => o.Item1)
                    .Merge()
                    .Select(localCache.Remove)
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
                    .SelectMany(distributedCache.Remove)
                    .Retry()
                    .Subscribe();
            }
        }
    }
}
