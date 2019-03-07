using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.LocalCache;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class LocalCacheFactoryExtensions
    {
        /// <summary>
        /// Adds an action to be executed each time a request to get items from the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onGetResult">The action to run each time a request to get items from the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onGetResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnGetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheGetResult> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }

        /// <summary>
        /// Adds an action to be executed each time a request to set items in the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onSetResult">The action to run each time a request to set items in the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onSetResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnSetResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a request to remove items from the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onRemoveResult">The action to run each time a request to remove items from the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onRemoveResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnRemoveResult(
            this ILocalCacheFactory cacheFactory,
            Action<CacheRemoveResult> onRemoveResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnRemoveResult(onRemoveResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time an exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time an exception occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnException(
            this ILocalCacheFactory cacheFactory,
            Action<CacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a matching exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Adds an action to be executed each time an exception of type <typeparamref name="TException"/> occurs while
        /// handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <typeparam name="TException">The <see cref="Exception"/> type (including derived types) to apply this action to</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory OnException<TException>(
            this ILocalCacheFactory cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a matching exception of type <typeparamref name="TException"/>
        /// occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <typeparam name="TException">The <see cref="Exception"/> type (including derived types) to apply this action to</typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Configures the key serializers that this cache factory will use
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="configAction">The config action to configure the key serializers</param>
        /// <returns></returns>
        public static ILocalCacheFactory WithKeySerializers(
            this ILocalCacheFactory cacheFactory,
            Action<KeySerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializers(configAction);
        }
        
        /// <summary>
        /// Adds an <see cref="ILocalCacheWrapperFactory"/> which adds a wrapper to each
        /// <see cref="ILocalCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static ILocalCacheFactory WithWrapper(
            this ILocalCacheFactory cacheFactory,
            ILocalCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
        }
        
        /// <summary>
        /// Adds a wrapper which tracks the count of pending requests. Use <see cref="PendingRequestsCounterContainer"/>
        /// to retrieve the counts
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="behaviour">How to add the wrapper which tracks the pending request count to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static ILocalCacheFactory WithPendingRequestsCounter(
            this ILocalCacheFactory cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        /// <summary>
        /// Swallows all exceptions thrown by the cache.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptions(this ILocalCacheFactory cacheFactory)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => true);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which match the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptions(
            this ILocalCacheFactory cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(predicate);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which are of type <typeparamref name="TException"/>
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <typeparam name="TException">The type of exception to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptions<TException>(this ILocalCacheFactory cacheFactory)
            where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which are of type <typeparamref name="TException"/> and match the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <typeparam name="TException">The type of exception to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptions<TException>(
            this ILocalCacheFactory cacheFactory,
            Func<TException, bool> predicate)
            where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception matches the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter inner exceptions</param>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptionsInner(
            this ILocalCacheFactory cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => predicate(ex.InnerException));
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception is of type <typeparamref name="TException"/>.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <typeparam name="TException">The type of the inner exceptions to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptionsInner<TException>(this ILocalCacheFactory cacheFactory)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException);
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception is of type <typeparamref name="TException"/> and matches the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter inner exceptions</param>
        /// <typeparam name="TException">The type of the inner exceptions to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory SwallowExceptionsInner<TException>(
            this ILocalCacheFactory cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Builds an <see cref="ILocalCache{TK,TV}"/> instance. It is unlikely that you will ever want to use
        /// this. See <see cref="BuildAsCache{TK,TV}(CacheMeIfYouCan.ILocalCacheFactory,string)"/> if you want to
        /// build a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static ILocalCache<TK, TV> Build<TK, TV>(
            this ILocalCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build<TK, TV>(cacheName);
        }
        
        /// <summary>
        /// Builds an <see cref="ICache{TK,TV}"/> instance. Use this to create a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this ILocalCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache<TK, TV>(cacheName);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a request to get items from the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onGetResult">The action to run each time a request to get items from the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onGetResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnGetResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheGetResult<TK, TV>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a request to set items in the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onSetResult">The action to run each time a request to set items in the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onSetResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a request to remove items from the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onRemoveResult">The action to run each time a request to remove items from the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onRemoveResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnRemoveResult<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheRemoveResult<TK>> onRemoveResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnRemoveResult(onRemoveResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time an exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time an exception occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a matching exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Adds an action to be executed each time an exception of type <typeparamref name="TException"/> occurs while
        /// handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <typeparam name="TException">The <see cref="Exception"/> type (including derived types) to apply this action to</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time a matching exception of type <typeparamref name="TException"/>
        /// occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <param name="onException">The action to run each time a matching error occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <typeparam name="TException">The <see cref="Exception"/> type (including derived types) to apply this action to</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
            where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed && predicate(typed)) onException(typed); }, behaviour);
        }

        /// <summary>
        /// Sets the <see cref="ISerializer"/> instance used to serialize and deserialize keys
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The key serializer</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ISerializer<TK> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer);
        }

        /// <summary>
        /// Sets the functions used to serialize and deserialize keys
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The function to serialize each key</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TK, string> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer);
        }
        
        /// <summary>
        /// Adds an <see cref="ILocalCacheWrapperFactory"/> which adds a wrapper to each
        /// <see cref="ILocalCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ILocalCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCacheWrapperFactoryToGenericAdapter<TK, TV>(wrapperFactory), behaviour);
        } 
        
        /// <summary>
        /// Adds an <see cref="ILocalCacheWrapperFactory{TK,TV}"/> which adds a wrapper to each
        /// <see cref="ILocalCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            ILocalCacheWrapperFactory<TK, TV> wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
        }
        
        /// <summary>
        /// Adds a wrapper which tracks the count of pending requests. Use <see cref="PendingRequestsCounterContainer"/>
        /// to retrieve the counts
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="behaviour">How to add the wrapper which tracks the pending request count to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> WithPendingRequestsCounter<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new LocalCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        /// <summary>
        /// Swallows all exceptions thrown by the cache.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV>(this ILocalCacheFactory<TK, TV> cacheFactory)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => true);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which match the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(predicate);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which are of type <typeparamref name="TException"/>
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <typeparam name="TException">The type of exception to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory)
            where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException);
        }
        
        /// <summary>
        /// Swallows exceptions thrown by the cache which are of type <typeparamref name="TException"/> and match the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter exceptions</param>
        /// <typeparam name="TException">The type of exception to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate)
            where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception matches the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter inner exceptions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<Exception, bool> predicate)
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => predicate(ex.InnerException));
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception is of type <typeparamref name="TException"/>.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <typeparam name="TException">The type of the inner exceptions to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException);
        }
        
        /// <summary>
        /// Swallows any exceptions where the inner exception is of type <typeparamref name="TException"/> and matches the predicate.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="predicate">The predicate used to filter inner exceptions</param>
        /// <typeparam name="TException">The type of the inner exceptions to swallow</typeparam>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Builds an <see cref="ILocalCache{TK,TV}"/> instance. It is unlikely that you will ever want to use
        /// this. See <see cref="BuildAsCache{TK,TV}(CacheMeIfYouCan.ILocalCacheFactory,string)"/> if you want to
        /// build a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static ILocalCache<TK, TV> Build<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build(cacheName);
        }
        
        /// <summary>
        /// Builds an <see cref="ICache{TK,TV}"/> instance. Use this to create a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache(cacheName);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the results of each request to get items from the
        /// cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onGetResult">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnGetResultObservable(
            this ILocalCacheFactory cacheFactory,
            Action<IObservable<CacheGetResult>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onGetResult, cacheFactory.OnGetResult, behaviour);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the results of each request to set items in the
        /// cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onSetResult">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnSetResultObservable(
            this ILocalCacheFactory cacheFactory,
            Action<IObservable<CacheSetResult>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onSetResult, cacheFactory.OnSetResult, behaviour);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the exceptions thrown by any calls to the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory OnExceptionObservable(
            this ILocalCacheFactory cacheFactory,
            Action<IObservable<CacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, cacheFactory.OnException, behaviour);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the results of each request to get items from the
        /// cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onGetResult">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheGetResult<TK, TV>>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onGetResult, cacheFactory.OnGetResult, behaviour);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the results of each request to set items in the
        /// cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onSetResult">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheSetResult<TK, TV>>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onSetResult, cacheFactory.OnSetResult, behaviour);
        }
        
        /// <summary>
        /// Creates and configures an observable sequence containing the exceptions thrown by any calls to the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to configure the observable</param>
        /// <param name="behaviour">How to add the action (which pushes items onto the observable) to the existing list of actions</param>
        /// <returns></returns>
        public static ILocalCacheFactory<TK, TV> OnExceptionObservable<TK, TV>(
            this ILocalCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, cacheFactory.OnException, behaviour);
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
