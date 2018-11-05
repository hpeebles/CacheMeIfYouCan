namespace CacheMeIfYouCan.Caches
{
    public class CachedItemCount
    {
        public readonly string CacheType;
        public readonly FunctionInfo FunctionInfo;
        public readonly long Count;

        internal CachedItemCount(string cacheType, FunctionInfo functionInfo, long count)
        {
            CacheType = cacheType;
            FunctionInfo = functionInfo;
            Count = count;
        }
    }
}