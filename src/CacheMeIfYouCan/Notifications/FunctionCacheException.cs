using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Notifications
{
    public abstract class FunctionCacheException<TK> : FunctionCacheException
    {
        internal FunctionCacheException(
            string functionName,
            IReadOnlyCollection<Key<TK>> keys,
            string message,
            Exception exception)
            : base(functionName, new Lazy<IReadOnlyCollection<string>>(() => keys.Select(k => k.AsStringSafe).ToList()), message, exception)
        {
            Keys = keys;
        }
        
        public new IReadOnlyCollection<Key<TK>> Keys { get; }
    }
    
    public abstract class FunctionCacheException : Exception
    {
        private readonly Lazy<IReadOnlyCollection<string>> _keys;

        internal FunctionCacheException(
            string functionName,
            Lazy<IReadOnlyCollection<string>> keys,
            string message,
            Exception exception)
            : base(message, exception)
        {
            FunctionName = functionName;
            _keys = keys;
        }

        public string FunctionName { get; }
        public IReadOnlyCollection<string> Keys => _keys.Value;
    }
}