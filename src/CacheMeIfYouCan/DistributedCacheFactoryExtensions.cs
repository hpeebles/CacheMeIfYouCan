using System;
using CacheMeIfYouCan.Configuration;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan
{
    public static class DistributedCacheFactoryExtensions
    {
        public static IDistributedCacheFactory OnGetResult(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheGetResult> onGetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, ordering);
        }

        public static IDistributedCacheFactory OnSetResult(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheSetResult> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, ordering);
        }
        
        public static IDistributedCacheFactory OnError(
            this IDistributedCacheFactory cacheFactory,
            Action<CacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, ordering);
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

        public static IDistributedCacheFactory AddWrapper(
            this IDistributedCacheFactory cacheFactory,
            IDistributedCacheWrapperFactory wrapper)
        {
            return cacheFactory
                .AsFactory()
                .AddWrapper(wrapper);
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
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnGetResult(onGetResult, ordering);
        }

        public static IDistributedCacheFactory<TK, TV> OnSetResult<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheSetResult<TK, TV>> onSetResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnSetResult(onSetResult, ordering);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnError<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<CacheException<TK>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return cacheFactory
                .AsFactory()
                .OnError(onError, ordering);
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
        
        public static IDistributedCacheFactory<TK, TV> AddWrapper<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            IDistributedCacheWrapperFactory<TK, TV> wrapper)
        {
            return cacheFactory
                .AsFactory()
                .AddWrapper(wrapper);
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
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnGetResult, ordering);
        }
        
        public static IDistributedCacheFactory OnSetResultObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheSetResult>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnSetResult, ordering);
        }
        
        public static IDistributedCacheFactory OnErrorObservable(
            this IDistributedCacheFactory cacheFactory,
            Action<IObservable<CacheException>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, cacheFactory.OnError, ordering);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnGetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheGetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnGetResult, ordering);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnSetResultObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheSetResult<TK, TV>>> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onResult, cacheFactory.OnSetResult, ordering);
        }
        
        public static IDistributedCacheFactory<TK, TV> OnErrorObservable<TK, TV>(
            this IDistributedCacheFactory<TK, TV> cacheFactory,
            Action<IObservable<CacheException<TK>>> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            return ObservablesHelper.SetupObservable(onError, cacheFactory.OnError, ordering);
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