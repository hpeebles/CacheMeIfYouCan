using System;

namespace CacheMeIfYouCan.Internal
{
    internal readonly struct ValueWithExpiry<T>
    {
        public T Value { get; }
        public DateTimeOffset Expiry { get; }

        public ValueWithExpiry(T value, DateTimeOffset expiry)
        {
            Value = value;
            Expiry = expiry;
        }
        
        public static implicit operator T(ValueWithExpiry<T> value)
        {
            return value.Value;
        }
    }
}