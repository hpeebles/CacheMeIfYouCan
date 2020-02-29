using System;
using System.Threading;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public interface ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, out TConfig>
        where TConfig : ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, TConfig>
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

    public interface ICachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, Task<TValue>> Build();
    }

    public interface ICachedFunctionConfigurationManagerAsync_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerAsync_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerAsync_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, CancellationToken, Task<TValue>> Build();
    }

    public interface ICachedFunctionConfigurationManagerAsyncCanx_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerAsyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, TValue> Build();
    }

    public interface ICachedFunctionConfigurationManagerSync_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerSync_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerSync_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, CancellationToken, TValue> Build();
    }

    public interface ICachedFunctionConfigurationManagerSyncCanx_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerSyncCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, ValueTask<TValue>> Build();
    }

    public interface ICachedFunctionConfigurationManagerValueTask_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerValueTask_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
    
    public interface ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> :
        ICachedFunctionConfigurationManagerBase<TParam, TKey, TValue, ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue>>
    {
        Func<TParam, CancellationToken, ValueTask<TValue>> Build();
    }

    public interface ICachedFunctionConfigurationManagerValueTaskCanx_1Param_KeySelector<TParam, TValue> :
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TParam, TValue>
    {
        ICachedFunctionConfigurationManagerValueTaskCanx_1Param<TParam, TKey, TValue> WithCacheKey<TKey>(Func<TParam, TKey> cacheKeySelector);
    }
}