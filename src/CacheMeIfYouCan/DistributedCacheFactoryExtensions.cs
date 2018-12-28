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
        
        public static IDistributedCacheFactory OnError(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, behaviour);
        }
        
        public static IDistributedCacheFactory OnError(
            this IDistributedCacheFactory cacheFactory,
            Func<CacheException, bool> predicate,
            Action<CacheException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (predicate(ex)) onError(ex); }, behaviour);
        }
        
        public static IDistributedCacheFactory OnError<TException>(
            this IDistributedCacheFactory cacheFactory,
            Action<TException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (ex is TException typed) onError(typed); }, behaviour);
        }
        
        public static IDistributedCacheFactory OnError<TException>(
            this IDistributedCacheFactory cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (ex is TException typed && predicate(typed)) onError(typed); }, behaviour);
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
        
        public static IDistributedCacheFactory<TK, TV> OnError<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnError<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<CacheException<TK>, bool> predicate,
            Action<CacheException<TK>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (predicate(ex)) onError(ex); }, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnError<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<TException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (ex is TException typed) onError(typed); }, behaviour);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnError<TK, TV, TException>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Func<TException, bool> predicate,
            Action<TException> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append) where TException : CacheException<TK>
        {
            return cacheFactory
                .AsFactory()
                .OnError(ex => { if (ex is TException typed && predicate(typed)) onError(typed); }, behaviour);
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
        
        public static IDistributedCacheFactory OnErrorObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheException>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, cacheFactory.OnError, behaviour);
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
        
        public static IDistributedCacheFactory<TK, TV> OnErrorObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheException<TK>>> onError,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            return ObservablesHelper.SetupObservable(onError, cacheFactory.OnError, behaviour);
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