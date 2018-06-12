using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly string Message;
        public readonly string KeyString;
        public readonly Exception Exception;

        protected internal FunctionCacheErrorEvent(string message, string keyString, Exception exception)
        {
            Message = message;
            KeyString = keyString;
            Exception = exception;
        }
    }

    public class FunctionCacheErrorEvent<TK> : FunctionCacheErrorEvent
    {
        public readonly TK Key;

        internal FunctionCacheErrorEvent(string message, TK key, string keyString, Exception exception)
            : base(message, keyString, exception)
        {
            Key = key;
        }
    }
}