using System;

namespace CacheMeIfYouCan.Configuration
{
    public interface ILocalCacheConfig<TK>
    {
        string CacheName { get; }
        Func<TK, string> KeySerializer { get; }
        KeyComparer<TK> KeyComparer { get; }
    }
}
