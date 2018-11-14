using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly string FunctionName;
        public readonly long Timestamp;
        public readonly string Message;
        public readonly Exception Exception;
        private readonly Lazy<IList<string>> _keys;

        internal FunctionCacheErrorEvent(
            string functionName,
            Lazy<IList<string>> keys,
            long timestamp,
            string message,
            Exception exception)
        {
            FunctionName = functionName;
            _keys = keys;
            Timestamp = timestamp;
            Message = message;
            Exception = exception;
        }

        public IList<string> Keys => _keys.Value;
    }

    public sealed class FunctionCacheErrorEvent<TK> : FunctionCacheErrorEvent
    {
        public new readonly IList<Key<TK>> Keys;

        internal FunctionCacheErrorEvent(
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