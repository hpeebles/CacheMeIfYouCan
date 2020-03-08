﻿using System;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public interface ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, out TConfig>
        where TConfig : ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        TConfig WithTimeToLive(TimeSpan timeToLive);
        TConfig WithTimeToLiveFactory(Func<TKey, TimeSpan> timeToLiveFactory);
        TConfig WithLocalCache(ILocalCache<TKey, TValue> cache);
        TConfig WithDistributedCache(IDistributedCache<TKey, TValue> cache);
        TConfig DisableCaching(bool disableCaching = true);
        TConfig SkipCacheWhen(Func<TKey, bool> predicate, SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet);
        TConfig SkipCacheWhen(Func<TKey, TValue, bool> predicate);
        TConfig SkipLocalCacheWhen(Func<TKey, bool> predicate, SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet);
        TConfig SkipLocalCacheWhen(Func<TKey, TValue, bool> predicate);
        TConfig SkipDistributedCacheWhen(Func<TKey, bool> predicate, SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet);
        TConfig SkipDistributedCacheWhen(Func<TKey, TValue, bool> predicate);
    }

    public interface ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerAsync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerSync_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
    
    public interface ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParam, TParam, TValue, ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>>,
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue>
    {
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_Success<TParam, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_Exception<TParam>> onException = null);
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
        ISingleKeyCachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> OnResult(
            Action<CachedFunctionWithSingleKeyResult_1Param_Success<TParam, TKey, TValue>> onSuccess = null,
            Action<CachedFunctionWithSingleKeyResult_1Param_Exception<TParam, TKey>> onException = null);
    }
}