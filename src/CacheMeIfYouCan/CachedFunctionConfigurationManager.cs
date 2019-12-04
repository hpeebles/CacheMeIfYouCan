using System;
using System.Threading.Tasks;
using CacheMeIfYouCan.Internal;
using CacheMeIfYouCan.Internal.CachedFunctions;

namespace CacheMeIfYouCan
{
    public sealed class CachedFunctionConfigurationManager<TKey, TValue>
    {
        private readonly CachedFunctionConfiguration<TKey, TValue> _config;

        internal CachedFunctionConfigurationManager(Func<TKey, Task<TValue>> originalFunc)
        {
            _config = new CachedFunctionConfiguration<TKey, TValue>(originalFunc);
        }

        public CachedFunctionConfigurationManager<TKey, TValue> WithTimeToLive(TimeSpan timeToLive)
        {
            return WithTimeToLiveFactory(_ => timeToLive);
        }

        public CachedFunctionConfigurationManager<TKey, TValue> WithTimeToLiveFactory(Func<TKey, TimeSpan> timeToLiveFactory)
        {
            _config.TimeToLiveFactory = timeToLiveFactory;
            return this;
        }
        
        public CachedFunctionConfigurationManager<TKey, TValue> WithLocalCache(ILocalCache<TKey, TValue> cache)
        {
            _config.LocalCache = cache;
            return this;
        }

        public Func<TKey, Task<TValue>> Build()
        {
            var cachedFunction = new CachedFunctionWithSingleKey<TKey, TValue>(_config);

            return cachedFunction.Get;
        }
    }
}