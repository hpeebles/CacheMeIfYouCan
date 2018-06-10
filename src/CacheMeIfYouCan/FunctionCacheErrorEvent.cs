using System;

namespace CacheMeIfYouCan
{
    public struct FunctionCacheErrorEvent
    {
        public readonly string Message;
        public readonly string Key;
        public readonly Exception Exception;

        internal FunctionCacheErrorEvent(string message, string key, Exception exception)
        {
            Message = message;
            Key = key;
            Exception = exception;
        }
    }
}