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
        
        public static ILocalCacheFactory OnException(
            this ILocalCacheFactory cacheFactory,
            Action<CacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
        public static ILocalCacheFactory OnException(
            this ILocalCacheFactory cacheFactory,
            Func<CacheException, bool> predicate,
            Action<CacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (predicate(ex)) onException(ex); }, behaviour);
        }
        
        public static ILocalCacheFactory OnException<TException>(
            this ILocalCacheFactory cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
        public static ILocalCacheFactory OnException<TException>(
            this ILocalCacheFactory cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed && predicate(typed)) onException(typed); }, behaviour);
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
        
        public static ILocalCacheFactory WithPendingRequestsCounter(
            this ILocalCacheFactory cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCachePendingRequestsCounterWrapperFactory(), behaviour);
        }

        public static ILocalCacheFactory SwallowExceptions(this ILocalCacheFactory cacheFactory)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => true);
        }
        
        public static ILocalCacheFactory SwallowExceptions(
            this ILocalCacheFactory cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(predicate);
        }
        
        public static ILocalCacheFactory SwallowExceptions<TException>(this ILocalCacheFactory cacheFactory)
            where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException);
        }
        
        public static ILocalCacheFactory SwallowExceptions<TException>(
            this ILocalCacheFactory cacheFactory,
            Func<TException, bool> predicate)
            where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException typed && predicate(typed));
        }
        
        public static ILocalCacheFactory SwallowExceptionsInner(
            this ILocalCacheFactory cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => predicate(ex.InnerException));
        }
        
        public static ILocalCacheFactory SwallowExceptionsInner<TException>(this ILocalCacheFactory cacheFactory)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException);
        }
        
        public static ILocalCacheFactory SwallowExceptionsInner<TException>(
            this ILocalCacheFactory cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
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
        
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<CacheException<TK>, bool> predicate,
            Action<CacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (predicate(ex)) onException(ex); }, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed && predicate(typed)) onException(typed); }, behaviour);
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
                .WithWrapper(new LocalCacheWrapperFactoryToGenericAdapter<TK, TV>(wrapperFactory), behaviour);
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
        
        public static ILocalCacheFactory<TK, TV> WithPendingRequestsCounter<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV>(this ILocalCacheFactory<TK, TV> cacheFactory)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => true);
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(predicate);
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException);
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException typed && predicate(typed));
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => predicate(ex.InnerException));
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException);
        }
        
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
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
        
        public static ILocalCacheFactory OnExceptionObservable(
            this ILocalCacheFactory configManager,
            Action<IObservable<CacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
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
        
        public static ILocalCacheFactory<TK, TV> OnExceptionObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> configManager,
            Action<IObservable<CacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, configManager.OnException, behaviour);
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