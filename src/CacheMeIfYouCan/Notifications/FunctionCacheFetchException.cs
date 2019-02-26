using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class FunctionCacheFetchException<TK> : FunctionCacheException<TK>
    {
        internal FunctionCacheFetchException(
            string functionName,
            IList<Key<TK>> keys,
            string message,
            Exception exception)
            : base(functionName, keys, message, exception)
        { }
    }
}