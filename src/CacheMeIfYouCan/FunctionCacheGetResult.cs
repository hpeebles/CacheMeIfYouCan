namespace CacheMeIfYouCan
{
    public abstract class FunctionCacheGetResult
    {
        public readonly string InterfaceName;
        public readonly string FunctionName;
        public readonly string KeyString;
        public readonly Outcome Outcome;
        public readonly long Duration;
        public readonly string CacheType;

        protected internal FunctionCacheGetResult(
            string interfaceName,
            string functionName,
            string keyString,
            Outcome outcome,
            long duration,
            string cacheType)
        {
            InterfaceName = interfaceName;
            FunctionName = functionName;
            KeyString = keyString;
            Outcome = outcome;
            Duration = duration;
            CacheType = cacheType;
        }
    }

    public class FunctionCacheGetResult<TK, TV> : FunctionCacheGetResult
    {
        public readonly TK Key;
        public readonly TV Value;

        internal FunctionCacheGetResult(
            string interfaceName,
            string functionName,
            TK key,
            TV value,
            string keyString,
            Outcome outcome,
            long duration,
            string cacheType)
            : base(interfaceName, functionName, keyString, outcome, duration, cacheType)
        {
            Key = key;
            Value = value;
        }
    }
    
    public enum Outcome
    {
        Error,
        FromCache,
        Fetch
    }
}