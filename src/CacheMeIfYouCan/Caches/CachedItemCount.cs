namespace CacheMeIfYouCan.Caches
{
    public class CachedItemCount
    {
        public readonly string CacheType;
        public readonly FunctionInfo FunctionInfo;
        public readonly int Count;

        internal CachedItemCount(string cacheType, FunctionInfo functionInfo, int count)
        {
            CacheType = cacheType;
            FunctionInfo = functionInfo;
            Count = count;
        }
    }
}