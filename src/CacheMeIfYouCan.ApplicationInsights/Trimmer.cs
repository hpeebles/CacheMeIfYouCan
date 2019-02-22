using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.ApplicationInsights
{
    internal class Trimmer
    {
        private readonly int _count;

        public Trimmer(int count)
        {
            _count = count;
        }

        public IEnumerable<T> Trim<T>(ICollection<T> input)
        {
            return input.Count <= _count ? input : input.Take(_count);
        }
    }
}