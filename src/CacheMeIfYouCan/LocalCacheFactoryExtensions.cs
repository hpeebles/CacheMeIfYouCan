using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class LocalCacheFactoryExtensions
    {
        public static ILocalCacheFactory OnGetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheGetResult> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }

        public static ILocalCacheFactory OnSetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        public static ILocalCacheFactory OnError(
            this ILocalCacheFactory cacheFactory,
            Action<CacheException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, behaviour);
        }

        public static ILocalCacheFactory WithKeySerializers(
            this ILocalCacheFactory cacheFactory,
            Action<KeySerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializers(configAction);
        }
        
        public static ILocalCacheFactory WithWrapper(
            this ILocalCacheFactory cacheFactory,
            ILocalCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
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
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }

        public static ILocalCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnError<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, behaviour);
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

        public static ILocalCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ILocalCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCacheWrapperFactoryToGenericAdaptor<TK, TV>(wrapperFactory), behaviour);
        } 
        
        public static ILocalCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ILocalCacheWrapperFactory<TK, TV> wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
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
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, behaviour);
        }
        
        public static ILocalCacheFactory OnSetResultObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheSetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, behaviour);
        }
        
        public static ILocalCacheFactory OnErrorObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheException>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnGetResult, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, configManager.OnSetResult, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnErrorObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, configManager.OnError, behaviour);
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