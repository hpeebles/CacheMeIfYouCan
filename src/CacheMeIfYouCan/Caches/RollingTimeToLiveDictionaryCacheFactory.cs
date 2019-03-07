using System;
using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Caches
{
    public class RollingTimeToLiveDictionaryCacheFactory : ILocalCacheFactory
    {
        private readonly TimeSpan _rollingTimeToLive;
        
        public RollingTimeToLiveDictionaryCacheFactory(TimeSpan rollingTimeToLive)
        {
            _rollingTimeToLive = rollingTimeToLive;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config)
        {
            return new RollingTimeToLiveDictionaryCache<TK, TV>(config.CacheName, _rollingTimeToLive, config.KeyComparer);
        }
    }
}