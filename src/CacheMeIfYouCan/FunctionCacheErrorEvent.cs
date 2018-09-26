using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly FunctionInfo FunctionInfo;
        public readonly string KeyString;
        public readonly long Timestamp;
        public readonly string Message;
        public readonly Exception Exception;

        protected internal FunctionCacheErrorEvent(
            FunctionInfo functionInfo,
            string keyString,
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
            TK key,
            string keyString,
            long timestamp,
            string message,
            Exception exception)
            : base(functionInfo, keyString, timestamp, message, exception)
        {
            Key = key;
        }
    }
}