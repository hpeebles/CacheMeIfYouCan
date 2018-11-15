using System;

namespace CacheMeIfYouCan.Configuration
{
    public class CacheFactoryConfig<TK, TV>
    {
        public string KeyspacePrefix;
        public Func<string, TK> KeyDeserializer;
        public Func<TV, string> ValueSerializer;
        public Func<string, TV> ValueDeserializer;
    }
}