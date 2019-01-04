using System;

namespace CacheMeIfYouCan.Internal
{
    internal static class CacheFactoryPresetKeyFactory
    {
        public static CacheFactoryPresetKey Create<T>(T value) where T : struct, Enum
        {
            return new CacheFactoryPresetKey(typeof(T), Convert.ToInt32(value));
        }
        
        public static CacheFactoryPresetKey Create(int value)
        {
            return new CacheFactoryPresetKey(typeof(int), value);
        }
    }
}