using System;

namespace CacheMeIfYouCan.Caches
{
    public class RollingTimeToLiveDictionaryCacheFactory : ILocalCacheFactory
    {
        private readonly TimeSpan _rollingTimeToLive;
        
        public RollingTimeToLiveDictionaryCacheFactory(TimeSpan rollingTimeToLive)
        {
            _rollingTimeToLive = rollingTimeToLive;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new RollingTimeToLiveDictionaryCache<TK, TV>(cacheName, _rollingTimeToLive);
        }
    }
}