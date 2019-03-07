using CacheMeIfYouCan.Configuration;

namespace CacheMeIfYouCan.Caches
{
    public class DictionaryCacheFactory : ILocalCacheFactory
    {
        private readonly int? _maxItems; 
        
        public DictionaryCacheFactory(int? maxItems = null)
        {
            _maxItems = maxItems;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(ILocalCacheConfig<TK> config)
        {
            return new DictionaryCache<TK, TV>(config.CacheName, config.KeyComparer, _maxItems);
        }
    }
}