using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Tests
{
    public static class BoolAndIntGenerator
    {
        public static IEnumerable<object[]> Generate(int start, int count)
        {
            foreach (var value in Enumerable.Range(start, count))
            {
                yield return new object[] { true, value };
                yield return new object[] { false, value };
            }
        }
    }
}