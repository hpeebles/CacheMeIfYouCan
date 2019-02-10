using System;

namespace CacheMeIfYouCan.Configuration
{
    [Flags]
    public enum SkipCacheSettings
    {
        SkipGet = 0b_0001,
        SkipSet = 0b_0010,
        SkipGetAndSet = 0b0011
    }
}