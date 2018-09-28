using System;

namespace CacheMeIfYouCan.Caches
{
    public interface ILocalCache<TK, TV>
    {
        GetFromCacheResult<TV> Get(Key<TK> key);
        void Set(Key<TK> key, TV value, TimeSpan timeToLive);
        void Remove(Key<TK> key);
    }
}