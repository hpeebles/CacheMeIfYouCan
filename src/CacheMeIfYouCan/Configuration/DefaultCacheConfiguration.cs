using System;
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
            var current = OnResult;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnResult = onResult;
            else if (ordering == ActionOrdering.Append)
                OnResult = x => { current(x); onResult(x); };
            else
                OnResult = x => { onResult(x); current(x); };
            
            return this;
        }

        public DefaultCacheConfiguration WithOnFetchAction(
            Action<FunctionCacheFetchResult> onFetch,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = OnFetch;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnFetch = onFetch;
            else if (ordering == ActionOrdering.Append)
                OnFetch = x => { current(x); onFetch(x); };
            else
                OnFetch = x => { onFetch(x); current(x); };

            return this;
        }

        public DefaultCacheConfiguration WithOnErrorAction(
            Action<FunctionCacheException> onError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = OnError;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnError = onError;
            else if (ordering == ActionOrdering.Append)
                OnError = x => { current(x); onError(x); };
            else
                OnError = x => { onError(x); current(x); };

            return this;
        }

        public DefaultCacheConfiguration WithOnCacheGetAction(
            Action<CacheGetResult> onCacheGet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = OnCacheGet;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnCacheGet = onCacheGet;
            else if (ordering == ActionOrdering.Append)
                OnCacheGet = x => { current(x); onCacheGet(x); };
            else
                OnCacheGet = x => { onCacheGet(x); current(x); };

            return this;
        }

        public DefaultCacheConfiguration WithOnCacheSetAction(
            Action<CacheSetResult> onCacheSet,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = OnCacheSet;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnCacheSet = onCacheSet;
            else if (ordering == ActionOrdering.Append)
                OnCacheSet = x => { current(x); onCacheSet(x); };
            else
                OnCacheSet = x => { onCacheSet(x); current(x); };

            return this;
        }

        public DefaultCacheConfiguration WithOnCacheErrorAction(
            Action<CacheException> onCacheError,
            ActionOrdering ordering = ActionOrdering.Append)
        {
            var current = OnCacheError;
            if (current == null || ordering == ActionOrdering.Overwrite)
                OnCacheError = onCacheError;
            else if (ordering == ActionOrdering.Append)
                OnCacheError = x => { current(x); onCacheError(x); };
            else
                OnCacheError = x => { onCacheError(x); current(x); };

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