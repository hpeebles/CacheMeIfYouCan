using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheException<TK> : FunctionCacheException
    {
        internal FunctionCacheException(
            string functionName,
            IList<Key<TK>> keys,
            string message,
            Exception exception)
            : base(functionName, new Lazy<IList<string>>(() => keys.Select(k => k.AsStringSafe).ToArray()), message, exception)
        {
            Keys = keys;
        }
        
        public new IList<Key<TK>> Keys { get; }
    }
    
    public abstract class FunctionCacheException : Exception
    {
        private readonly Lazy<IList<string>> _keys;

        internal FunctionCacheException(
            string functionName,
            Lazy<IList<string>> keys,
            string message,
            Exception exception)
            : base(message, exception)
        {
            FunctionName = functionName;
            _keys = keys;
        }

        public string FunctionName { get; }
        public IList<string> Keys => _keys.Value;
    }
}