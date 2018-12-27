using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan
{
    public interface ILocalCache<TK, TV> : IDisposable
    {
        string CacheName { get; }
        string CacheType { get; }
        GetFromCacheResult<TK, TV> Get(Key<TK> key);
        void Set(Key<TK> key, TV value, TimeSpan timeToLive);
        IList<GetFromCacheResult<TK, TV>> Get(ICollection<Key<TK>> keys);
        void Set(ICollection<KeyValuePair<Key<TK>, TV>> values, TimeSpan timeToLive);
        void Remove(Key<TK> key);
    }
}