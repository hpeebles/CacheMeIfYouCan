using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class DistributedCacheFactoryExtensions
    {
        public static IDistributedCacheFactory OnGetResult(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheGetResult> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }

        public static IDistributedCacheFactory OnSetResult(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        public static IDistributedCacheFactory OnException(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
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
        
        public static IDistributedCacheFactory OnException<TException>(
            this IDistributedCacheFactory cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
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

        public static IDistributedCacheFactory WithKeySerializers(
            this IDistributedCacheFactory cacheFactory,
            Action<KeySerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializers(configAction);
        }
        
        public static IDistributedCacheFactory WithValueSerializers(
            this IDistributedCacheFactory cacheFactory,
            Action<ValueSerializers> configAction)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializers(configAction);
        }
        
        public static IDistributedCacheFactory WithKeyspacePrefix(
            this IDistributedCacheFactory cacheFactory,
            string keyspacePrefix)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefix);
        }
        
        public static IDistributedCacheFactory WithKeyspacePrefix(
            this IDistributedCacheFactory cacheFactory,
            Func<string, string> keyspacePrefixFunc)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefixFunc);
        }

        public static IDistributedCacheFactory WithWrapper(
            this IDistributedCacheFactory cacheFactory,
            IDistributedCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
        }
        
        public static IDistributedCacheFactory WithPendingRequestsCounter(
            this IDistributedCacheFactory cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        public static IDistributedCache<TK, TV> Build<TK, TV>(
            this IDistributedCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build<TK, TV>(cacheName);
        }

        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this IDistributedCacheFactory cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache<TK, TV>(cacheName);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnGetResult<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheGetResult<TK, TV>> onGetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, behaviour);
        }

        public static IDistributedCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnException(onException, behaviour);
        }
        
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
        
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed) onException(typed); }, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnException<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnException(ex => { if (ex is TException typed && predicate(typed)) onException(typed); }, behaviour);
        }

        public static IDistributedCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer.Serialize, serializer.Deserialize<TK>);
        }

        public static IDistributedCacheFactory<TK, TV> WithKeySerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TK, string> serializer,
            Func<string, TK> deserializer)
        {
            return cacheFactory
                .AsFactory()
                .WithKeySerializer(serializer, deserializer);
        }

        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer<TV> serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer.Serialize, serializer.Deserialize);
        }

        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            ISerializer serializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer.Serialize, serializer.Deserialize<TV>);
        }

        public static IDistributedCacheFactory<TK, TV> WithValueSerializer<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TV, string> serializer,
            Func<string, TV> deserializer)
        {
            return cacheFactory
                .AsFactory()
                .WithValueSerializer(serializer, deserializer);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithKeyspacePrefix<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            string keyspacePrefix)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(x => keyspacePrefix);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithKeyspacePrefix<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<string, string> keyspacePrefixFunc)
        {
            return cacheFactory
                .AsFactory()
                .WithKeyspacePrefix(keyspacePrefixFunc);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IDistributedCacheWrapperFactory wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCacheWrapperFactoryToGenericAdapter<TK, TV>(wrapperFactory), behaviour);
        } 
        
        public static IDistributedCacheFactory<TK, TV> WithWrapper<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IDistributedCacheWrapperFactory<TK, TV> wrapperFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(wrapperFactory, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> WithPendingRequestsCounter<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .WithWrapper(new DistributedCachePendingRequestsCounterWrapperFactory(), behaviour);
        }
        
        public static IDistributedCache<TK, TV> Build<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .Build(cacheName);
        }
        
        public static ICache<TK, TV> BuildAsCache<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            string cacheName)
        {
            return cacheFactory
                .AsFactory()
                .BuildAsCache(cacheName);
        }
        
        public static IDistributedCacheFactory OnGetResultObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheGetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnGetResult, behaviour);
        }
        
        public static IDistributedCacheFactory OnSetResultObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheSetResult>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnSetResult, behaviour);
        }
        
        public static IDistributedCacheFactory OnExceptionObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheException>> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onException, cacheFactory.OnException, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnGetResult, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnSetResult, behaviour);
        }
        
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