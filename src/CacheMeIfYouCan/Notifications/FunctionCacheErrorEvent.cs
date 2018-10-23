using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly long Timestamp;
        public readonly string Message;
        public readonly Exception Exception;
        private readonly Lazy<IList<string>> _keys;

        internal FunctionCacheErrorEvent(
            FunctionInfo functionInfo,
            Lazy<IList<string>> keys,
            long timestamp,
            string message,
            Exception exception)
        {
            FunctionInfo = functionInfo;
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
            FunctionInfo functionInfo,
            IList<Key<TK>> keys,
            long timestamp,
            string message,
            Exception exception)
            : base(functionInfo, new Lazy<IList<string>>(() => keys.Select(k => (string)k).ToArray()), timestamp, message, exception)
        {
            Keys = keys;
        }
    }
}