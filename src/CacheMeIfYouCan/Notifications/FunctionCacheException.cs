using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheException : Exception
    {
        public readonly string FunctionName;
        public readonly long Timestamp;
        private readonly Lazy<IList<string>> _keys;

        internal FunctionCacheException(
            string functionName,
            Lazy<IList<string>> keys,
            long timestamp,
            string message,
            Exception exception)
        : base(message, exception)
        {
            FunctionName = functionName;
            _keys = keys;
            Timestamp = timestamp;
        }

        public IList<string> Keys => _keys.Value;
    }

    public sealed class FunctionCacheException<TK> : FunctionCacheException
    {
        public new readonly IList<Key<TK>> Keys;

        internal FunctionCacheException(
            string functionName,
            IList<Key<TK>> keys,
            long timestamp,
            string message,
            Exception exception)
            : base(functionName, new Lazy<IList<string>>(() => keys.Select(k => (string)k).ToArray()), timestamp, message, exception)
        {
            Keys = keys;
        }
    }
}