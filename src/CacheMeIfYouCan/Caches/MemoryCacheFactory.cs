namespace CacheMeIfYouCan.Caches
{
    public class MemoryCacheFactory : ILocalCacheFactory
    {
        public bool RequiresStringKeys => true;

        public ILocalCache<TK, TV> Build<TK, TV>(FunctionInfo functionInfo)
        {
            return new MemoryCache<TK, TV>(functionInfo);
        }
    }
}
