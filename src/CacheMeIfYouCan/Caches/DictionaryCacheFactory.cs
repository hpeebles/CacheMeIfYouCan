namespace CacheMeIfYouCan.Caches
{
    public class DictionaryCacheFactory : ILocalCacheFactory
    {
        private readonly int? _maxItems; 
        
        public DictionaryCacheFactory(int? maxItems = null)
        {
            _maxItems = maxItems;
        }
        
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return new DictionaryCache<TK, TV>(cacheName, _maxItems);
        }
    }
}