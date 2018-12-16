using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class FunctionCacheFetchException<TK> : FunctionCacheException<TK>
    {
        internal FunctionCacheFetchException(
            string functionName,
            IList<Key<TK>> keys,
            long timestamp,
            string message,
            Exception exception)
            : base(functionName, keys, timestamp, message, exception)
        { }
    }
}