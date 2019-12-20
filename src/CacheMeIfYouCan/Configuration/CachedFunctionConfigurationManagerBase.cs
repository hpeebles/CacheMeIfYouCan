using System;
using System.Threading;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan.Configuration
{
    public abstract class CachedFunctionConfigurationManagerBase<TKey, TValue, TConfig>
        where TConfig : CachedFunctionConfigurationManagerBase<TKey, TValue, TConfig>
    {
        private readonly CachedFunctionConfiguration<TKey, TValue> _config;

        internal CachedFunctionConfigurationManagerBase(Func<TKey, CancellationToken, Task<TValue>> originalFunc)
        {
            _config = new CachedFunctionConfiguration<TKey, TValue>(originalFunc);
        }

        public TConfig WithTimeToLive(TimeSpan timeToLive)
        {
            return WithTimeToLiveFactory(_ => timeToLive);
        }

        public TConfig WithTimeToLiveFactory(Func<TKey, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = timeToLiveFactory;
            return (TConfig)this;
        }
        
        public TConfig WithLocalCache(ILocalCache<TKey, TValue> cache)
        {
            _config.LocalCache = cache;
            return (TConfig)this;
        }

        public TConfig WithDistributedCache(IDistributedCache<TKey, TValue> cache)
        {
            _config.DistributedCache = cache;
            return (TConfig)this;
        }

        public TConfig DisableCaching(bool disableCaching = true)
        {
            _config.DisableCaching = disableCaching;
            return (TConfig)this;
        }
        
        public TConfig SkipCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipCacheGetPredicate = _config.SkipCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipCacheSetPredicate = _config.SkipCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipLocalCacheGetPredicate = _config.SkipLocalCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipLocalCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipLocalCacheSetPredicate = _config.SkipLocalCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }
        
        public TConfig SkipDistributedCacheWhen(
            Func<TKey, bool> predicate,
            SkipCacheWhen when = CacheMeIfYouCan.SkipCacheWhen.SkipCacheGetAndCacheSet)
        {
            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheGet))
                _config.SkipDistributedCacheGetPredicate = _config.SkipDistributedCacheGetPredicate.Or(predicate);

            if (when.HasFlag(CacheMeIfYouCan.SkipCacheWhen.SkipCacheSet))
                _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or((key, value) => predicate(key));

            return (TConfig)this;
        }

        public TConfig SkipDistributedCacheWhen(Func<TKey, TValue, bool> predicate)
        {
            _config.SkipDistributedCacheSetPredicate = _config.SkipDistributedCacheSetPredicate.Or(predicate);
            return (TConfig)this;
        }

        private protected CachedFunctionWithSingleKey<TKey, TValue> BuildCachedFunction()
        {
            return new CachedFunctionWithSingleKey<TKey, TValue>(_config);
        }
    }
}