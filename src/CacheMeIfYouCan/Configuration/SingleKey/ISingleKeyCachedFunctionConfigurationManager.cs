using System;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public interface ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, out TConfig>
        where TConfig : ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        TConfig WithTimeToLive(TimeSpan timeToLive);
        TConfig WithLocalCache(ILocalCache<TKey, TValue> cache);
        TConfig WithDistributedCache(IDistributedCache<TKey, TValue> cache);
        TConfig DisableCaching(bool disableCaching = true);
        TConfig DontGetFromCacheWhen(Func<TKey, bool> predicate);
        TConfig DontStoreInCacheWhen(Func<TKey, TValue, bool> predicate);
        TConfig DontGetFromLocalCacheWhen(Func<TKey, bool> predicate);
        TConfig DontStoreInLocalCacheWhen(Func<TKey, TValue, bool> predicate);
        TConfig DontGetFromDistributedCacheWhen(Func<TKey, bool> predicate);
        TConfig DontStoreInDistributedCacheWhen(Func<TKey, TValue, bool> predicate);
        TConfig OnResult(Action<SuccessfulRequestEvent<TParams, TKey, TValue>> onSuccess = null, Action<ExceptionEvent<TParams, TKey>> onException = null);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsync_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>
    { 
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerSync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerSync_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerSync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>>,
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithTimeToLiveFactory(Func<TParam, TimeSpan> timeToLiveFactory);
    }
}