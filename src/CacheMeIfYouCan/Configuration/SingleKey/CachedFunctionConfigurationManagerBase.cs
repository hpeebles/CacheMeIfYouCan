using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Events.CachedFunction.SingleKey;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;
using CacheMeIfYouCan.Internal.CachedFunctions.Configuration;

namespace CacheMeIfYouCan.Configuration.SingleKey
{
    public abstract class CachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig> :
        ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
        where TConfig : class, ISingleKeyCachedFunctionConfigurationManagerBase<TParams, TKey, TValue, TConfig>
    {
        private readonly CachedFunctionWithSingleKeyConfiguration<TParams, TKey, TValue> _config =
            new CachedFunctionWithSingleKeyConfiguration<TParams, TKey, TValue>();

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            _config.TimeToLive = timeToLive;
            return ThisAsTConfig();
        }

        private protected TConfig WithTimeToLiveFactoryInternal(Func<TParams, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = timeToLiveFactory;
            return ThisAsTConfig();
        }
        
        public TConfig WithLocalCache(ILocalCache<TKey, TValue> cache)
        {
            _config.LocalCache = cache;
            return ThisAsTConfig();
        }

        public TConfig WithDistributedCache(IDistributedCache<TKey, TValue> cache)
        {
            _config.DistributedCache = cache;
            return ThisAsTConfig();
        }

        public TConfig DisableCaching(bool disableCaching = true)
        {
            _config.DisableCaching = disableCaching;
            return ThisAsTConfig();
        }

        private protected TConfig DontGetFromCacheWhenInternal(Func<TParams, bool> predicate)
        {
            _config.SkipCacheGetPredicate = _config.SkipCacheGetPredicate.Or(predicate);
            return ThisAsTConfig();
        }
        
        private protected TConfig DontStoreInCacheWhenInternal(Func<TParams, TValue, bool> predicate)
        {
            _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or(predicate);
            return ThisAsTConfig();
        }

        public TConfig DontGetFromLocalCacheWhen(Func<TKey, bool> predicate)
        {
            _config.SkipLocalCacheGetPredicate = _config.SkipLocalCacheGetPredicate.Or(predicate);
            return ThisAsTConfig();
        }
        
        public TConfig DontStoreInLocalCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or(predicate);
            return ThisAsTConfig();
        }
        
        public TConfig DontGetFromDistributedCacheWhen(Func<TKey, bool> predicate)
        {
            _config.SkipDistributedCacheGetPredicate = _config.SkipDistributedCacheGetPredicate.Or(predicate);
            return ThisAsTConfig();
        }

        public TConfig DontStoreInDistributedCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or(predicate);
            return ThisAsTConfig();
        }

        public TConfig OnResult(
            Action<SuccessfulRequestEvent<TParams, TKey, TValue>> onSuccess = null,
            Action<ExceptionEvent<TParams, TKey>> onException = null)
        {
            if (!(onSuccess is null))
                _config.OnSuccessAction += onSuccess;
            
            if (!(onException is null))
                _config.OnExceptionAction += onException;

            return ThisAsTConfig();
        }

        private protected CachedFunctionWithSingleKey<TParams, TKey, TValue> BuildCachedFunction(
            Func<TParams, CancellationToken, ValueTask<TValue>> originalFunction,
            Func<TParams, TKey> cacheKeySelector)
        {
            return new CachedFunctionWithSingleKey<TParams, TKey, TValue>(originalFunction, cacheKeySelector, _config);
        }

        private TConfig ThisAsTConfig()
        {
            return (TConfig)(object)this;
        }
    }
}