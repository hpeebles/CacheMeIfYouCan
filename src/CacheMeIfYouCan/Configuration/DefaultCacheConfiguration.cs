using System;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCacheConfiguration
    {
        internal TimeSpan TimeToLive { get; private set; } = TimeSpan.FromHours(1);
        internal bool EarlyFetchEnabled { get; private set; } = true;
        internal bool DisableCache { get; private set; }
        internal ILocalCacheFactory LocalCacheFactory { get; private set; }
        internal IDistributedCacheFactory DistributedCacheFactory { get; private set; }
        internal Action<FunctionCacheGetResult> OnResult { get; private set; }
        internal Action<FunctionCacheFetchResult> OnFetch { get; private set; }
        internal Action<FunctionCacheException> OnError { get; private set; }
        internal Action<CacheGetResult> OnCacheGet { get; private set; }
        internal Action<CacheSetResult> OnCacheSet { get; private set; }
        internal Action<CacheException> OnCacheError { get; private set; }
        internal readonly KeySerializers KeySerializers = new KeySerializers();
        internal readonly ValueSerializers ValueSerializers = new ValueSerializers();

        public DefaultCacheConfiguration WithTimeToLive(TimeSpan timeToLive)
        {
            TimeToLive = timeToLive;
            return this;
        }

        public DefaultCacheConfiguration WithEarlyFetchEnabled(bool earlyFetchEnabled)
        {
            EarlyFetchEnabled = earlyFetchEnabled;
            return this;
        }

        public DefaultCacheConfiguration WithCacheDisabled(bool disableCache)
        {
            DisableCache = disableCache;
            return this;
        }

        public DefaultCacheConfiguration WithLocalCacheFactory(ILocalCacheFactory localCacheFactory)
        {
            LocalCacheFactory = localCacheFactory;
            return this;
        }

        public DefaultCacheConfiguration WithDistributedCacheFactory(IDistributedCacheFactory distributedCacheFactory)
        {
            DistributedCacheFactory = distributedCacheFactory;
            return this;
        }

        public DefaultCacheConfiguration WithOnResultAction(
            Action<FunctionCacheGetResult> onResult,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnResult = ActionsHelper.Combine(OnResult, onResult, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithOnFetchAction(
            Action<FunctionCacheFetchResult> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnFetch = ActionsHelper.Combine(OnFetch, onFetch, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithOnErrorAction(
            Action<FunctionCacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnError = ActionsHelper.Combine(OnError, onError, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheGetAction(
            Action<CacheGetResult> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnCacheGet = ActionsHelper.Combine(OnCacheGet, onCacheGet, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheSetAction(
            Action<CacheSetResult> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnCacheSet = ActionsHelper.Combine(OnCacheSet, onCacheSet, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheErrorAction(
            Action<CacheException> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            OnCacheError = ActionsHelper.Combine(OnCacheError, onCacheError, ordering);
            return this;
        }

        public DefaultCacheConfiguration WithKeySerializers(Action<KeySerializers> config)
        {
            config(KeySerializers);
            return this;
        }

        public DefaultCacheConfiguration WithValueSerializers(Action<ValueSerializers> config)
        {
            config(ValueSerializers);
            return this;
        }
    }
    
    public static class DefaultCacheConfig
    {
        public static readonly DefaultCacheConfiguration Configuration = new DefaultCacheConfiguration();
    }
}