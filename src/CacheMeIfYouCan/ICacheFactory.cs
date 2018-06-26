using System;

namespace CacheMeIfYouCan
{
    public interface ICacheFactory
    {
        ICache<T> Build<T>(MemoryCache<T> memoryCache = null, Func<T, string> serializer = null, Func<string, T> deserializer = null);
    }
}