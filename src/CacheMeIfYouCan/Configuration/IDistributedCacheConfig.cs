using System;

namespace CacheMeIfYouCan.Configuration
{
    public interface IDistributedCacheConfig<TK, TV> : ILocalCacheConfig<TK>
    {
        string KeyspacePrefix { get; }
        Func<string, TK> KeyDeserializer { get; }
        Func<TV, string> ValueSerializer { get; }
        Func<string, TV> ValueDeserializer { get; }
        Func<TV, byte[]> ValueByteSerializer { get; }
        Func<byte[], TV> ValueByteDeserializer { get; }

        void Validate();
    }
}
