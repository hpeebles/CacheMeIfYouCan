using System;
using System.Collections.Generic;
using System.Linq;

namespace CacheMeIfYouCan.Tests.Generators
{
    public static class EnumAndIntGenerator<TEnum>
    {
        public static IEnumerable<object[]> Generate(int start, int count)
        {
            foreach (var enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
                foreach (var intValue in Enumerable.Range(start, count))
                    yield return new object[] { enumValue, intValue };
        }
    }
}