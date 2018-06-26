using System;

namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheErrorEvent
    {
        public readonly string InterfaceName;
        public readonly string FunctionName;
        public readonly string KeyString;
        public readonly string Message;
        public readonly Exception Exception;

        protected internal FunctionCacheErrorEvent(
            string interfaceName,
            string functionName,
            string keyString,
            string message, 
            Exception exception)
        {
            InterfaceName = interfaceName;
            FunctionName = functionName;
            KeyString = keyString;
            Message = message;
            Exception = exception;
        }
    }

    public class FunctionCacheErrorEvent<TK> : FunctionCacheErrorEvent
    {
        public readonly TK Key;

        internal FunctionCacheErrorEvent(
            string interfaceName,
            string functionName,
            TK key,
            string keyString,
            string message,
            Exception exception)
            : base(interfaceName, functionName, keyString, message, exception)
        {
            Key = key;
        }
    }
}