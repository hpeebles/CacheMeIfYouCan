using System;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly Lazy<string> KeyString;
        public readonly long Timestamp;
        public readonly string Message;
        public readonly Exception Exception;

        protected internal FunctionCacheErrorEvent(
            FunctionInfo functionInfo,
            Lazy<string> keyString,
            long timestamp,
            string message,
            Exception exception)
        {
            FunctionInfo = functionInfo;
            KeyString = keyString;
            Timestamp = timestamp;
            Message = message;
            Exception = exception;
        }
    }

    public class FunctionCacheErrorEvent<TK> : FunctionCacheErrorEvent
    {
        public readonly TK Key;

        internal FunctionCacheErrorEvent(
            FunctionInfo functionInfo,
            Key<TK> key,
            long timestamp,
            string message,
            Exception exception)
            : base(functionInfo, key.AsString, timestamp, message, exception)
        {
            Key = key;
        }
    }
}