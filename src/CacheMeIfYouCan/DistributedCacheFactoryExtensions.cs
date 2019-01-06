using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class DistributedCacheFactoryExtensions
    {
        /// <summary>
        /// Adds an action to be executed each time a request to get items from the cache completes
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onGetResult">The action to run each time a request to get items from the cache completes</param>
        /// <param name="behaviour">How to add the <paramref name="onGetResult"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static IDistributedCacheFactory OnGetResult(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnSetResult(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time an exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time an exception occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static IDistributedCacheFactory OnException(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnException(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnException<TException>(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnException<TException>(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory WithKeySerializers(
            this IDistributedCacheFactory cacheFactory,
            Action<KeySerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializers(configAction);
        }
        
        /// <summary>
        /// Configures the value serializers that this cache factory will use
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="configAction">The config action to configure the value serializers</param>
        /// <returns></returns>
        public static IDistributedCacheFactory WithValueSerializers(
            this IDistributedCacheFactory cacheFactory,
            Action<ValueSerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializers(configAction);
        }
        
        /// <summary>
        /// Sets the keyspace prefix to be prepended to each key in the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="keyspacePrefix">The keyspace prefix to be prepended to each key in the cache</param>
        /// <returns></returns>
        public static IDistributedCacheFactory WithKeyspacePrefix(
            this IDistributedCacheFactory cacheFactory,
            string keyspacePrefix)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefix);
        }
        
        /// <summary>
        /// Sets the keyspace prefix generating function which takes the cache name and produces the prefix to be
        /// prepended to each key in the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="keyspacePrefixFunc">A function which takes the cache name and produces the keyspace prefix to
        /// be prepended to each key in the cache</param>
        /// <returns></returns>
        public static IDistributedCacheFactory WithKeyspacePrefix(
            this IDistributedCacheFactory cacheFactory,
            Func<string, string> keyspacePrefixFunc)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefixFunc);
        }

        /// <summary>
        /// Adds an <see cref="IDistributedCacheWrapperFactory"/> which adds a wrapper to each
        /// <see cref="IDistributedCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static IDistributedCacheFactory WithWrapper(
            this IDistributedCacheFactory cacheFactory,
            IDistributedCacheWrapperFactory wrapperFactory,
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
        public static IDistributedCacheFactory WithPendingRequestsCounter(
            this IDistributedCacheFactory cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        /// <summary>
        /// Adds a wrapper which prevents duplicate requests from being sent to the cache. When a duplicate is found we
        /// await the original rather than sending the duplicate
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="behaviour">How to add the wrapper which catches the duplicate requests to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static IDistributedCacheFactory WithDuplicateRequestCatching(
            this IDistributedCacheFactory cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCacheDuplicateRequestCatchingWrapperFactory(), behaviour);
        }
        
        /// <summary>
        /// Swallows all exceptions thrown by the cache.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <returns></returns>
        public static IDistributedCacheFactory SwallowExceptions(this IDistributedCacheFactory cacheFactory)
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
        public static IDistributedCacheFactory SwallowExceptions(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory SwallowExceptions<TException>(
            this IDistributedCacheFactory cacheFactory)
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
        public static IDistributedCacheFactory SwallowExceptions<TException>(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory SwallowExceptionsInner(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory SwallowExceptionsInner<TException>(
            this IDistributedCacheFactory cacheFactory)
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
        public static IDistributedCacheFactory SwallowExceptionsInner<TException>(
            this IDistributedCacheFactory cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Builds an <see cref="IDistributedCache{TK,TV}"/> instance. It is unlikely that you will ever want to use
        /// this. See <see cref="BuildAsCache{TK,TV}(CacheMeIfYouCan.IDistributedCacheFactory,string)"/> if you want to
        /// build a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static IDistributedCache<TK, TV> Build<TK, TV>(
            this IDistributedCacheFactory cacheFactory,
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
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnGetResult<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        /// <summary>
        /// Adds an action to be executed each time an exception occurs while handling a cache request
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="onException">The action to run each time an exception occurs while handling a cache request</param>
        /// <param name="behaviour">How to add the <paramref name="onException"/> action to the existing list of actions</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer.Serialize, serializer.Deserialize<TK>);
        }
        
        /// <summary>
        /// Sets the <see cref="ISerializer{T}"/> instance used to serialize and deserialize keys
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The key serializer</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer<TK> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer.Serialize, serializer.Deserialize);
        }

        /// <summary>
        /// Sets the functions used to serialize and deserialize keys
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The function to serialize each key</param>
        /// <param name="deserializer">The function to deserialize each key</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TK, string> serializer,
            Func<string, TK> deserializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer, deserializer);
        }

        /// <summary>
        /// Sets the <see cref="ISerializer"/> instance used to serialize and deserialize values
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The value serializer</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer.Serialize, serializer.Deserialize<TV>);
        }

        /// <summary>
        /// Sets the <see cref="ISerializer{T}"/> instance used to serialize and deserialize values
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The value serializer</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer<TV> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer.Serialize, serializer.Deserialize);
        }
        
        /// <summary>
        /// Sets the functions used to serialize and deserialize values
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="serializer">The function to serialize each value</param>
        /// <param name="deserializer">The function to deserialize each value</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TV, string> serializer,
            Func<string, TV> deserializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer, deserializer);
        }
        
        /// <summary>
        /// Sets the keyspace prefix to be prepended to each key in the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="keyspacePrefix">The keyspace prefix to be prepended to each key in the cache</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithKeyspacePrefix<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            string keyspacePrefix)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(x => keyspacePrefix);
        }
        
        /// <summary>
        /// Sets the keyspace prefix generating function which takes the cache name and produces the prefix to be
        /// prepended to each key in the cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="keyspacePrefixFunc">A function which takes the cache name and produces the keyspace prefix to
        /// be prepended to each key in the cache</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithKeyspacePrefix<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<string, string> keyspacePrefixFunc)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefixFunc);
        }
        
        /// <summary>
        /// Adds an <see cref="IDistributedCacheWrapperFactory"/> which adds a wrapper to each
        /// <see cref="IDistributedCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IDistributedCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCacheWrapperFactoryToGenericAdapter<TK, TV>(wrapperFactory), behaviour);
        } 
        
        /// <summary>
        /// Adds an <see cref="IDistributedCacheWrapperFactory{TK,TV}"/> which adds a wrapper to each
        /// <see cref="IDistributedCache{TK,TV}"/> instance built by this factory
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="wrapperFactory">The wrapper factory</param>
        /// <param name="behaviour">How to add the <paramref name="wrapperFactory"/> to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IDistributedCacheWrapperFactory<TK, TV> wrapperFactory,
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
        public static IDistributedCacheFactory<TK, TV> WithPendingRequestsCounter<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCachePendingRequestsCounterWrapperFactory(), behaviour);
        }

        /// <summary>
        /// Adds a wrapper which prevents duplicate requests from being sent to the cache. When a duplicate is found we
        /// await the original rather than sending the duplicate
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <param name="behaviour">How to add the wrapper which catches the duplicate requests to the existing list of wrapperFactories</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> WithDuplicateRequestCatching<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCacheDuplicateRequestCatchingWrapperFactory());
        }
        
        /// <summary>
        /// Swallows all exceptions thrown by the cache.
        /// Any OnException actions are run before the exceptions are swallowed
        /// </summary>
        /// <param name="cacheFactory">The cache factory being configured</param>
        /// <returns></returns>
        public static IDistributedCacheFactory<TK, TV> SwallowExceptions<TK, TV>(this IDistributedCacheFactory<TK, TV> cacheFactory)
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptions<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory)
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptions<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory)
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
        public static IDistributedCacheFactory<TK, TV> SwallowExceptionsInner<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate)
            where TException : Exception
        {
            return cacheFactory
                .AsFactory()
                .SwallowExceptions(ex => ex.InnerException is TException typed && predicate(typed));
        }
        
        /// <summary>
        /// Builds an <see cref="IDistributedCache{TK,TV}"/> instance. It is unlikely that you will ever want to use
        /// this. See <see cref="BuildAsCache{TK,TV}(CacheMeIfYouCan.IDistributedCacheFactory{TK,TV},string)"/> if you
        /// want to build a standalone cache
        /// </summary>
        /// <param name="cacheFactory">The cache factory</param>
        /// <param name="cacheName">The name to be assigned to the newly created cache</param>
        /// <typeparam name="TK">The type of the cache key</typeparam>
        /// <typeparam name="TV">The type of the cache value</typeparam>
        /// <returns></returns>
        public static IDistributedCache<TK, TV> Build<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory OnGetResultObservable(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnSetResultObservable(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory OnExceptionObservable(
            this IDistributedCacheFactory cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
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
        public static IDistributedCacheFactory<TK, TV> OnExceptionObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheException<TK>>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, cacheFactory.OnException, behaviour);
        }

        private static DistributedCacheFactory AsFactory(this IDistributedCacheFactory cacheFactory)
        {
            if (cacheFactory is DistributedCacheFactory cf)
                return cf;
            
            return new DistributedCacheFactory(cacheFactory);
        }
        
        private static DistributedCacheFactory<TK, TV> AsFactory<TK, TV>(this IDistributedCacheFactory<TK, TV> cacheFactory)
        {
            if (cacheFactory is DistributedCacheFactory<TK, TV> cf)
                return cf;
            
            return new DistributedCacheFactory<TK, TV>(cacheFactory);
        }
    }
}
