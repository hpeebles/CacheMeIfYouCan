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
    
    internal sealed class NullCache<TOuterKey, TInnerKey, TValue> : ICache<TOuterKey, TInnerKey, TValue>
    {
        private NullCache() { }
        
        public static readonly NullCache<TOuterKey, TInnerKey, TValue> Instance = new NullCache<TOuterKey, TInnerKey, TValue>();
        
        public ValueTask<IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>>> GetMany(TOuterKey outerKey, IReadOnlyCollection<TInnerKey> innerKeys)
        {
            return default;
        }

        public ValueTask SetMany(TOuterKey outerKey, IReadOnlyCollection<KeyValuePair<TInnerKey, TValue>> values, TimeSpan timeToLive)
        {
            return default;
        }
    }
}