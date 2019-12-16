using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CacheMeIfYouCan.Internal
{
    internal sealed class NullCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private NullCache() { }
        
        public static readonly NullCache<TKey, TValue> Instance = new NullCache<TKey, TValue>();
        
        public ValueTask<(bool Success, TValue Value)> TryGet(TKey key) => default;
        public ValueTask Set(TKey key, TValue value, TimeSpan timeToLive) => default;
        public ValueTask<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetMany(IReadOnlyCollection<TKey> keys) => default;
        public ValueTask SetMany(IReadOnlyCollection<KeyValuePair<TKey, TValue>> values, TimeSpan timeToLive) => default;
    }
}