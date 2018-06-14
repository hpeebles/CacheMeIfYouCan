namespace CacheMeIfYouCan.Internal
{
    internal class MemoryCacheBuilder
    {
        public static MemoryCache<T> Build<T>(int maxSizeMB)
        {
            return new MemoryCache<T>(maxSizeMB);
        }
    }
}