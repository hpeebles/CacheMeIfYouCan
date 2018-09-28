using System;

namespace CacheMeIfYouCan.Caches
{
    public class CacheFactoryConfig<TK, TV>
    {
        public FunctionInfo FunctionInfo;
        public Func<string, TK> KeyDeserializer;
        public Func<TV, string> ValueSerializer;
        public Func<string, TV> ValueDeserializer;
    }
}