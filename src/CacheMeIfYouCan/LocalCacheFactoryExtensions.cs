using System;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class LocalCacheFactoryExtensions
    {
        public static ILocalCacheFactory OnGetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheGetResult> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, ordering);
        }

        public static ILocalCacheFactory OnSetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, ordering);
        }
        
        public static ILocalCacheFactory OnError(
            this ILocalCacheFactory cacheFactory,
            Action<CacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, ordering);
        }

        public static ILocalCacheFactory WithKeySerializers(
            this ILocalCacheFactory cacheFactory,
            Action<KeySerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializers(configAction);
        }
        
        public static ILocalCache<TK, TV> Build<TK, TV>(
            this ILocalCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build<TK, TV>(cacheName);
        }
        
        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this ILocalCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache<TK, TV>(cacheName);
        }
        
        public static ILocalCacheFactory<TK, TV> OnGetResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheGetResult<TK, TV>> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, ordering);
        }

        public static ILocalCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, ordering);
        }
        
        public static ILocalCacheFactory<TK, TV> OnError<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, ordering);
        }

        public static ILocalCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ISerializer<TK> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer);
        }
        
        public static ILocalCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TK, string> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer);
        }

        public static ILocalCache<TK, TV> Build<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build(cacheName);
        }
        
        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache(cacheName);
        }
        
        public static ILocalCacheFactory OnGetResultObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheGetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static ILocalCacheFactory OnSetResultObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheSetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static ILocalCacheFactory OnErrorObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        public static ILocalCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, ordering);
        }
        
        public static ILocalCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, ordering);
        }
        
        public static ILocalCacheFactory<TK, TV> OnErrorObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, ordering);
        }
        
        private static LocalCacheFactory AsFactory(this ILocalCacheFactory cacheFactory)
        {
            if (cacheFactory is LocalCacheFactory cf)
                return cf;
            
            return new LocalCacheFactory(cacheFactory);
        }
        
        private static LocalCacheFactory<TK, TV> AsFactory<TK, TV>(this ILocalCacheFactory<TK, TV> cacheFactory)
        {
            if (cacheFactory is LocalCacheFactory<TK, TV> cf)
                return cf;
            
            return new LocalCacheFactory<TK, TV>(cacheFactory);
        }
    }
}