namespace CacheMeIfYouCan.Internal
{
    internal class NullLocalCacheFactory : ILocalCacheFactory
    {
        public ILocalCache<TK, TV> Build<TK, TV>(string cacheName)
        {
            return null;
        }
    }
    
    internal class NullLocalCacheFactory<TK, TV> : ILocalCacheFactory<TK, TV>
    {
        public ILocalCache<TK, TV> Build(string cacheName)
        {
            return null;
        }
    }
}