using System;
using System.Collections.Generic;

namespace CacheMeIfYouCan.Notifications
{
    public sealed class FunctionCacheGetException<TK> : FunctionCacheException<TK>
    {
        internal FunctionCacheGetException(
            string functionName,
            IList<Key<TK>> keys,
            string message,
            Exception exception)
            : base(functionName, keys, message, exception)
        { }
    }
}