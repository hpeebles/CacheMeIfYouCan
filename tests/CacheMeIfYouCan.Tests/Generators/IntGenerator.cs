using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Tests.Generators
{
    public static class IntGenerator<TEnum>
    {
        public static IEnumerable<object[]> Generate(int start, int count)
        {
            foreach (var intValue in Enumerable.Range(start, count))
                yield return new object[] { intValue };
        }
    }
}