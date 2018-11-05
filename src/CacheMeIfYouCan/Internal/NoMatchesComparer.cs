﻿using System.Collections.Generic;

namespace CacheMeIfYouCan.Internal
{
    public class NoMatchesComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return false;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}