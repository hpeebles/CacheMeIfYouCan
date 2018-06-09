using System;

namespace CacheMeIfYouCan.Internal
{
    internal struct ValueWithExpiry<T>
    {
        public T Value;
        public DateTimeOffset Expiry;

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