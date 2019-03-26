using System;
using System.Collections.Generic;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Notifications;
using CacheMeIfYouCan.Serializers;

namespace CacheMeIfYouCan.Configuration
{
    public class DefaultCacheConfiguration
    {
        internal Func<TimeSpan> TimeToLiveFactory { get; private set; } = () => TimeSpan.FromHours(1);
        internal Func<TimeSpan> LocalCacheTimeToLiveOverride { get; private set; }
        internal bool DisableCache { get; private set; }
        internal bool DuplicateRequestCatchingEnabled { get; private set; }
        internal ILocalCacheFactory LocalCacheFactory { get; private set; }
        internal IDistributedCacheFactory DistributedCacheFactory { get; private set; }
        internal Action<FunctionCacheGetResult> OnResultAction { get; private set; }
        internal Action<FunctionCacheFetchResult> OnFetchAction { get; private set; }
        internal Action<FunctionCacheException> OnExceptionAction { get; private set; }
        internal Action<CacheGetResult> OnCacheGetAction { get; private set; }
        internal Action<CacheSetResult> OnCacheSetAction { get; private set; }
        internal Action<CacheRemoveResult> OnCacheRemoveAction { get; private set; }
        internal Action<CacheException> OnCacheExceptionAction { get; private set; }
        internal string KeyParamSeparator { get; private set; } = "_";
        internal int MaxFetchBatchSize { get; private set; }
        internal BatchBehaviour BatchBehaviour { get; private set; }
        internal bool ShouldFillMissingKeysWithDefaultValues { get; private set; }
        internal StoreInLocalCacheWhen ShouldOnlyStoreInLocalCacheWhen { get; private set; }
        internal KeySerializers KeySerializers { get; } = new KeySerializers();
        internal ValueSerializers ValueSerializers { get; } = new ValueSerializers();
        internal EqualityComparers KeyComparers { get; } = new EqualityComparers();
        
        internal Dictionary<CacheFactoryPresetKey, (ILocalCacheFactory local, IDistributedCacheFactory distributed)> CacheFactoryPresets { get; }
            = new Dictionary<CacheFactoryPresetKey, (ILocalCacheFactory, IDistributedCacheFactory)>();

        public DefaultCacheConfiguration WithTimeToLive(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            TimeToLiveFactory = () => timeToLive;

            if (jitterPercentage > 0)
                TimeToLiveFactory = TimeToLiveFactory.WithJitter(jitterPercentage);
            
            return this;
        }

        public DefaultCacheConfiguration WithLocalCacheTimeToLiveOverride(TimeSpan timeToLive, double jitterPercentage = 0)
        {
            LocalCacheTimeToLiveOverride = () => timeToLive;

            if (jitterPercentage > 0)
                LocalCacheTimeToLiveOverride = LocalCacheTimeToLiveOverride.WithJitter(jitterPercentage);
            
            return this;
        }

        public DefaultCacheConfiguration WithCacheDisabled(bool disableCache)
        {
            DisableCache = disableCache;
            return this;
        }

        public DefaultCacheConfiguration CatchDuplicateRequests(bool catchDuplicateRequests)
        {
            DuplicateRequestCatchingEnabled = catchDuplicateRequests;
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

        public DefaultCacheConfiguration OnResult(
            Action<FunctionCacheGetResult> onResult,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnResultAction = ActionsHelper.Combine(OnResultAction, onResult, behaviour);
            return this;
        }

        public DefaultCacheConfiguration OnFetch(
            Action<FunctionCacheFetchResult> onFetch,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnFetchAction = ActionsHelper.Combine(OnFetchAction, onFetch, behaviour);
            return this;
        }

        public DefaultCacheConfiguration OnException(
            Action<FunctionCacheException> onException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnExceptionAction = ActionsHelper.Combine(OnExceptionAction, onException, behaviour);
            return this;
        }

        public DefaultCacheConfiguration OnCacheGet(
            Action<CacheGetResult> onCacheGet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheGetAction = ActionsHelper.Combine(OnCacheGetAction, onCacheGet, behaviour);
            return this;
        }

        public DefaultCacheConfiguration OnCacheSet(
            Action<CacheSetResult> onCacheSet,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheSetAction = ActionsHelper.Combine(OnCacheSetAction, onCacheSet, behaviour);
            return this;
        }
        
        public DefaultCacheConfiguration OnCacheRemove(
            Action<CacheRemoveResult> onCacheRemove,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheRemoveAction = ActionsHelper.Combine(OnCacheRemoveAction, onCacheRemove, behaviour);
            return this;
        }

        public DefaultCacheConfiguration OnCacheException(
            Action<CacheException> onCacheException,
            AdditionBehaviour behaviour = AdditionBehaviour.Append)
        {
            OnCacheExceptionAction = ActionsHelper.Combine(OnCacheExceptionAction, onCacheException, behaviour);
            return this;
        }

        public DefaultCacheConfiguration WithKeyParamSeparator(string separator)
        {
            if (String.IsNullOrEmpty(separator))
                throw new ArgumentException(nameof(separator));

            KeyParamSeparator = separator;
            return this;
        }
        
        public DefaultCacheConfiguration WithBatchedFetches(int maxBatchSize, BatchBehaviour behaviour = BatchBehaviour.FillBatchesEvenly)
        {
            if (maxBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize));
            
            MaxFetchBatchSize = maxBatchSize;
            BatchBehaviour = behaviour;
            return this;
        }
        
        public DefaultCacheConfiguration FillMissingKeysWithDefaultValues(bool fillMissingKeysWithDefaultValues = true)
        {
            ShouldFillMissingKeysWithDefaultValues = fillMissingKeysWithDefaultValues;
            return this;
        }

        public DefaultCacheConfiguration OnlyStoreInLocalCacheWhen(StoreInLocalCacheWhen when)
        {
            ShouldOnlyStoreInLocalCacheWhen = when;
            return this;
        }

        public DefaultCacheConfiguration WithKeySerializers(Action<KeySerializers> configAction)
        {
            configAction(KeySerializers);
            return this;
        }

        public DefaultCacheConfiguration WithValueSerializers(Action<ValueSerializers> configAction)
        {
            configAction(ValueSerializers);
            return this;
        }
        
        public DefaultCacheConfiguration WithKeyComparer<T>(IEqualityComparer<T> comparer)
        {
            KeyComparers.Set(comparer);
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