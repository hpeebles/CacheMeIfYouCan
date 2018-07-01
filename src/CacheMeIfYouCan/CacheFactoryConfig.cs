using System;

namespace CacheMeIfYouCan
{
    public class CacheFactoryConfig<T>
    {
        public MemoryCache<T> MemoryCache;
        public FunctionInfo FunctionInfo;
        public Func<T, string> Serializer;
        public Func<string, T> Deserializer;
    }
}