using System;
using System.Collections.Generic;
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
        internal Action<FunctionCacheException> OnException { get; private set; }
        internal Action<CacheGetResult> OnCacheGet { get; private set; }
        internal Action<CacheSetResult> OnCacheSet { get; private set; }
        internal Action<CacheException> OnCacheException { get; private set; }
        internal KeySerializers KeySerializers { get; } = new KeySerializers();
        internal ValueSerializers ValueSerializers { get; } = new ValueSerializers();
        
        internal Dictionary<CacheFactoryPresetKey, (ILocalCacheFactory local, IDistributedCacheFactory distributed)> CacheFactoryPresets { get; }
            = new Dictionary<CacheFactoryPresetKey, (ILocalCacheFactory, IDistributedCacheFactory)>();

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
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnResult = ActionsHelper.Combine(OnResult, onResult, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithOnFetchAction(
            Action<FunctionCacheFetchResult> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnFetch = ActionsHelper.Combine(OnFetch, onFetch, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithOnExceptionAction(
            Action<FunctionCacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnException = ActionsHelper.Combine(OnException, onException, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheGetAction(
            Action<CacheGetResult> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheGet = ActionsHelper.Combine(OnCacheGet, onCacheGet, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheSetAction(
            Action<CacheSetResult> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheSet = ActionsHelper.Combine(OnCacheSet, onCacheSet, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithOnCacheExceptionAction(
            Action<CacheException> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheException = ActionsHelper.Combine(OnCacheException, onCacheException, behaviour);
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
        
        public DefaultCacheConfiguration CreateCacheFactoryPreset(
            int id,
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory)
        {
            var key = CacheFactoryPresetKeyFactory.Create(id);

            return CreateCacheFactoryPresetImpl(key, localCacheFactory, distributedCacheFactory);
        }
        
        public DefaultCacheConfiguration CreateCacheFactoryPreset<TEnum>(
            TEnum id,
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory)
            where TEnum : struct, Enum
        {
            var key = CacheFactoryPresetKeyFactory.Create(id);

            return CreateCacheFactoryPresetImpl(key, localCacheFactory, distributedCacheFactory);
        }

        private DefaultCacheConfiguration CreateCacheFactoryPresetImpl(
            CacheFactoryPresetKey key,
            ILocalCacheFactory localCacheFactory,
            IDistributedCacheFactory distributedCacheFactory)
        {
            CacheFactoryPresets.Add(key, (localCacheFactory, distributedCacheFactory));
            return this;
        }
    }
}