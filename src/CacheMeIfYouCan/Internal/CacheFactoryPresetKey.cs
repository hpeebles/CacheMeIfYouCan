using System;

namespace CacheMeIfYouCan.Internal
{
    internal readonly struct CacheFactoryPresetKey : IEquatable<CacheFactoryPresetKey>
    {
        public CacheFactoryPresetKey(Type type, int intValue)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = intValue;
        }

        public Type Type { get; }
        public int Value { get; }

        public bool Equals(CacheFactoryPresetKey other)
        {
            return Type.Equals(other.Type) && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CacheFactoryPresetKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ Value;
            }
        }

        public override string ToString()
        {
            return $"Type: '{Type.Name}'. Value: '{Value}'";
        }
        
        public static bool operator ==(CacheFactoryPresetKey left, CacheFactoryPresetKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CacheFactoryPresetKey left, CacheFactoryPresetKey right)
        {
            return !left.Equals(right);
        }
    }
}