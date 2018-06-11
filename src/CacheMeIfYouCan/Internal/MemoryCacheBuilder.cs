namespace CacheMeIfYouCan.Internal
{
    internal static class MemoryCacheBuilder
    {
        public static ICache<T> Build<T>(int maxSizeMB)
        {
            return new MemoryCache<T>(maxSizeMB);
        }
    }
}