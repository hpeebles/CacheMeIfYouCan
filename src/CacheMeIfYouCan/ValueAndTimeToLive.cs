using System;

namespace CacheMeIfYouCan
{
    public readonly struct ValueAndTimeToLive<T>
    {
        public ValueAndTimeToLive(T value, TimeSpan timeToLive)
        {
            Value = value;
            TimeToLive = timeToLive;
        }

        public T Value { get; }
        public TimeSpan TimeToLive { get; }

        public static implicit operator T(ValueAndTimeToLive<T> valueAndTimeToLive) => valueAndTimeToLive.Value;
    }
}