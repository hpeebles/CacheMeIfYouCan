using System;

namespace CacheMeIfYouCan.Redis
{
    [Flags]
    public enum KeyEvents
    {
        None = 0,
        Set = 0b_0001,
        Del = 0b_0010,
        Evicted = 0b_0100,
        Expired = 0b_1000,
        All = 0b_1111
    }
}