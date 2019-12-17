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

        private protected CachedFunctionWithSingleKey<TKey, TValue> BuildCachedFunction()
        {
            return new CachedFunctionWithSingleKey<TKey, TValue>(_config);
        }
    }
}