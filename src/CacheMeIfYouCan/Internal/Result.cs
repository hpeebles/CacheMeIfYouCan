using CacheMeIfYouCan.Notifications;

namespace CacheMeIfYouCan.Internal
{
    internal struct Result<TV>
    {
        public readonly TV Value;
        public readonly Outcome Outcome;
        public readonly string CacheType;

        public Result(TV value, Outcome outcome, string cacheType)
        {
            Value = value;
            Outcome = outcome;
            CacheType = cacheType;
        }
    }
}